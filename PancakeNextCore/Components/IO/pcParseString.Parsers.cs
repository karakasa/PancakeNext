using Eto.Drawing;
using Eto.Forms;
using Eto.Typesetting;
using Grasshopper2.Data;
using Grasshopper2.Types.Colour;
using PancakeNextCore.GH;
using PancakeNextCore.GH.Params;
using PancakeNextCore.GH.Params.AssocConverters;
using PancakeNextCore.Utility;
using PancakeNextCore.Utility.Polyfill;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Render;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Eto.Drawing.Color;
using Path = System.IO.Path;

namespace PancakeNextCore.Components.IO;
public sealed partial class pcParseString
{
    private abstract class Parser(string name, bool excludedInAll = false)
    {
        public string Name { get; } = name;
        public bool ExcludeInAllMode { get; } = excludedInAll;
        public abstract bool TryConvertGeneric(string from, [NotNullWhen(true)] ref object? outObj, [NotNullWhen(true)] ref string? outTypeName);
        public abstract bool TryParseStringTreeAs(Tree<string> strings, [NotNullWhen(true)] ref ITree? outputs, [NotNullWhen(true)] ref string? outTypeName, bool failOnInvalid = false);
    }
    private abstract class Parser<T>(string name, bool excludedInAll = false) : Parser(name.ToLowerInvariant(), excludedInAll)
    {
        public override sealed bool TryParseStringTreeAs(Tree<string> strings, [NotNullWhen(true)] ref ITree? outputs, [NotNullWhen(true)] ref string? outTypeName, bool failOnInvalid = false)
        {
            var array = new Twig<T>[strings.PathCount];
            for (int i = 0; i < array.Length; i++)
            {
                var twig = strings.Twigs[i];
                if (twig is null)
                {
                    array[i] = Garden.TwigEmpty<T>();
                }
                else
                {
                    var t = ParseStringsAs(twig, failOnInvalid);
                    if (t is null)
                    {
                        if (failOnInvalid)
                        {
                            return false;
                        }
                        else
                        {
                            t = Garden.TwigEmpty<T>();
                        }
                    }

                    array[i] = t;
                }
            }

            outputs = Garden.TreeFromTwigs(strings.Paths, array);
            outTypeName = Name;

            return true;
        }

        private Twig<T>? ParseStringsAs(Twig<string> strings, bool failOnInvalid)
        {
            var factory = new TwigFactory<T>(strings.LeafCount);

            foreach (var it in strings.Pears)
            {
                if (it is null)
                {
                    factory.AddNull();
                    continue;
                }

                var meta = it.Meta;
                var str = it.Item;
                if (TryParse(str, out var v))
                {
                    factory.Add(v, meta, false);
                }
                else
                {
                    if (failOnInvalid && !string.IsNullOrEmpty(str))
                    {
                        return null;
                    }

                    factory.Add(default!, meta, true);
                }
            }

            return factory.Create();
        }

        public override sealed bool TryConvertGeneric(string from, [NotNullWhen(true)] ref object? outObj, [NotNullWhen(true)] ref string? outTypeName)
        {
            if (TryParse(from, out var obj))
            {
                outObj = obj;
                outTypeName = Name;
                return true;
            }

            return false;
        }

        protected abstract bool TryParse(string strData, [NotNullWhen(true)] out T result);
    }

    static readonly Parser_Json JsonParser;
    static readonly Parser_XML XmlParser;
    static readonly Parser_Point3d Point3dParser;
    static readonly Parser_Integer IntParser;
    static readonly Parser_Number DoubleParser;
    static readonly Parser_Bool BoolParser;

    static readonly Parser[] Parsers = [
        new Parser_Null(),
        IntParser = new Parser_Integer(),
        BoolParser = new Parser_Bool(),
        DoubleParser = new Parser_Number(),
        Point3dParser = new(),
        JsonParser = new Parser_Json(),
        XmlParser = new Parser_XML(),
        new Parser_Color(),
        new Parser_DecimalLength(),
        new Parser_FtInchLength(),
        new Parser_Interval(),
        new Parser_DateTime(),
        new Parser_Guid(),
        new Parser_Quantity()
        ];
    private sealed class Parser_XML() : Parser<GhAssocBase>("xml")
    {
        protected override bool TryParse(string str, [NotNullWhen(true)] out GhAssocBase result)
        {
            if (!IsXmlFast(str))
            {
                result = null;
                return false;
            }

            try
            {
                var assoc = XmlIo.ReadXml(str, out _);
                result = assoc;
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        private static bool IsXmlFast(string xml)
        {
            if (xml.Length < 6) return false;
            if (xml[0] != '<' || xml[xml.Length - 1] != '>') return false;
            if (xml.StartsWith("<?xml version=\"", StringComparison.Ordinal))
            {
                if (xml.Length < 28) return false;

                var ind = xml.IndexOf("?>", 5, 100, StringComparison.Ordinal);
                if (ind < 0) return false;
                ind = xml.IndexOf("<", ind + 1, 20, StringComparison.Ordinal);
                if (ind < 0) return false;

                if (xml.Length < ind + 4) return false;
                var firstChar = xml[ind + 1];
                return XmlIo.IsAllowedFirstChar(firstChar);
            }
            else
            {
                return XmlIo.IsAllowedFirstChar(xml[1]);
            }
        }
    }
    private sealed class Parser_Null() : Parser<object?>("null")
    {
        protected override bool TryParse(string str, [NotNullWhen(true)] out object? result)
        {
            result = null;
            return str is "null" or "";
        }
    }
    private sealed class Parser_CommaString() : Parser<string>("CommaString")
    {
        protected override bool TryParse(string strData, [NotNullWhen(true)] out string result)
        {
            if (strData.StartsWith("\"") && strData.EndsWith("\""))
            {
                result = strData.Substring(1, strData.Length - 2);
                return true;
            }
            result = default;
            return false;
        }
    }
    private sealed class Parser_Point3d() : Parser<Point3d>("Point")
    {
        private struct ParseExecutor : IStringPartExecutor
        {
            private double x;
            private double y;
            private double z;
            private bool threeDimensional;
            public readonly Point3d CollectResult() => new(x, y, z);
            public bool HandlePart(string str, int partId, int startIndex, int length)
            {
                switch (partId)
                {
                    case 0:
                        return str.TrimmedEquals(startIndex, length, '{', '(');
                    case 1:
                        return str.TryParseTrimmedAsDouble(startIndex, length, out x);
                    case 2:
                        return str.TrimmedEquals(startIndex, length, ',');
                    case 3:
                        return str.TryParseTrimmedAsDouble(startIndex, length, out y);
                    case 4:
                        var kind = str.TrimmedEquals(startIndex, length, '}', ')', ',');
                        threeDimensional = kind == 2;
                        return kind >= 0;
                    case 5 when threeDimensional:
                        return str.TryParseTrimmedAsDouble(startIndex, length, out z);
                    case 6 when threeDimensional:
                        return str.TrimmedEquals(startIndex, length, '}');
                    default:
                        return false;
                }
            }
        }
        internal bool IsValid(string strData) => TryParse(strData, out _);
        protected override bool TryParse(string strData, [NotNullWhen(true)] out Point3d point)
        {
            var executor = new ParseExecutor();

            if (!strData.TrySplitWhile(default(StringUtility.IsNumericAndNegativePredicate), ref executor))
            {
                point = default;
                return false;
            }

            point = executor.CollectResult();
            return true;
        }
    }
    private sealed class Parser_Interval() : Parser<Interval>("Domain")
    {
        private struct ParseExecutor : IStringPartExecutor
        {
            private double x;
            private double y;
            public readonly Interval CollectResult() => new(x, y);
            public bool HandlePart(string str, int partId, int startIndex, int length)
            {
                switch (partId)
                {
                    case 0:
                        return str.TryParseSubstrAsDouble(startIndex, length, out x);
                    case 1:
                        str.TrimToPosition(ref startIndex, ref length);
                        return length switch
                        {
                            2 => StringSpanOperations.EqualsSubstring(str, startIndex, length, "To") ||
                                 StringSpanOperations.EqualsSubstring(str, startIndex, length, "->"),
                            1 => str[startIndex] == '~',
                            _ => false,
                        };
                    case 2:
                        return str.TryParseSubstrAsDouble(startIndex, length, out y1);
                    default:
                        return false;
                }
            }
        }
        protected override bool TryParse(string strData, [NotNullWhen(true)] out Interval domain)
        {
            ParseExecutor executor = default;
            if (!strData.TrySplitWhile(default(StringUtility.IsNumericAndNegativePredicate), ref executor))
            {
                domain = default;
                return false;
            }

            domain = executor.CollectResult();
            return true;
        }
    }
    private sealed class Parser_Color() : Parser<Colour>("Colour")
    {
        protected override bool TryParse(string strData, [NotNullWhen(true)] out Colour color)
        {
            if (strData.Length is not (7 or 9) || !strData.StartsWith("#", StringComparison.Ordinal))
            {
                color = null;
                return false;
            }

            if (!strData.TryParseSubstrAsInt(1, 2, out var part1, NumberStyles.HexNumber) ||
                !strData.TryParseSubstrAsInt(2, 2, out var part2, NumberStyles.HexNumber) ||
                !strData.TryParseSubstrAsInt(3, 2, out var part3, NumberStyles.HexNumber))
            {
                color = null;
                return false;
            }

            int part4;

            if (strData.Length == 9)
            {
                if (!strData.TryParseSubstrAsInt(7, 2, out part4, NumberStyles.HexNumber))
                {
                    color = null;
                    return false;
                }
            }
            else
            {
                part4 = 0;
            }

            if (strData.Length == 9)
            {
                if (part1 <= 100)
                {
                    // ARGB
                    color = Colour.FromEto(Color.FromArgb(part1, part2, part3, part4));
                }
                else if (part4 <= 100)
                {
                    // RGBA
                    color = Colour.FromEto(Color.FromArgb(part4, part1, part2, part3));
                }
                else
                {
                    color = default;
                    return false;
                }
            }
            else
            {
                color = Colour.FromEto(Color.FromArgb(part1, part2, part3));
            }

            return true;
        }
    }
    private sealed class Parser_Json() : Parser<GhAssocBase>("Json")
    {
        protected override bool TryParse(string strData, [NotNullWhen(true)] out GhAssocBase result)
            => BuiltinJsonParser.TryParseJsonLight(strData, out result);
    }
    private sealed class Parser_Quantity() : Parser<GhQuantity>("Quantity", true)
    {
        protected override bool TryParse(string strData, [NotNullWhen(true)] out GhQuantity result)
        {
            if (GhLengthDecimal.TryParse(strData, out var len))
            {
                result = len;
                return true;
            }

            if (GhLengthFeetInch.TryParse(strData, out var ft))
            {
                result = ft;
                return true;
            }

            result = null;
            return false;
        }
    }
    private sealed class Parser_DecimalLength() : Parser<GhLengthDecimal>("DecimalLength")
    {
        protected override bool TryParse(string strData, [NotNullWhen(true)] out GhLengthDecimal result)
            => GhLengthDecimal.TryParse(strData, out result);
    }
    private sealed class Parser_FtInchLength() : Parser<GhLengthFeetInch>("FeetInchLength")
    {
        protected override bool TryParse(string strData, [NotNullWhen(true)] out GhLengthFeetInch result)
            => GhLengthFeetInch.TryParse(strData, out result);
    }
    private sealed class Parser_Bool() : Parser<bool>("Boolean")
    {
        protected override bool TryParse(string strData, [NotNullWhen(true)] out bool result)
        {
            if (string.Equals(strData, "true", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                return true;
            }
            else if (string.Equals(strData, "false", StringComparison.OrdinalIgnoreCase))
            {
                result = false;
                return true;
            }

            return result = false;
        }
    }
    private sealed class Parser_Integer() : Parser<int>("Integer") { protected override bool TryParse(string strData, [NotNullWhen(true)] out int result) => int.TryParse(strData, out result); }
    private sealed class Parser_Number() : Parser<double>("Number") { protected override bool TryParse(string strData, [NotNullWhen(true)] out double result) => double.TryParse(strData, out result); }
    private sealed class Parser_DateTime() : Parser<DateTime>("DateTime") { protected override bool TryParse(string strData, [NotNullWhen(true)] out DateTime result) => DateTime.TryParse(strData, out result); }
    private sealed class Parser_Guid() : Parser<Guid>("Guid") { protected override bool TryParse(string strData, [NotNullWhen(true)] out Guid result) => Guid.TryParse(strData, out result); }
#if NET
    private abstract class GenericParser<T>(string name) : Parser<T>(name) where T : IParsable<T>
    {
        protected override bool TryParse(string strData, [NotNullWhen(true)] out T result) => T.TryParse(strData, null, out result);
    }
#endif
    private static bool TryHandleFile(string path, [NotNullWhen(true)] ref object? outObj, [NotNullWhen(true)] ref string? outTypeName)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        string result;
        switch (extension)
        {
            case ".json":
                result = FileIo.ReadAllText(path);
                if (JsonParser.TryConvertGeneric(result, ref outObj, ref outTypeName))
                    return true;
                break;
            case ".xml":
                result = FileIo.ReadAllText(path);
                if (XmlParser.TryConvertGeneric(result, ref outObj, ref outTypeName))
                    return true;
                break;
        }

        outObj = null;
        return false;
    }
    private enum EducatedGuess
    {
        Unknown,
        Empty,
        Bool,
        Integer,
        Number,
        TupleOf3Numbers
    }
    private static EducatedGuess GuessString(string str)
    {
        if (str.Length >= 4 && (string.Equals(str, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(str, "false", StringComparison.OrdinalIgnoreCase)))
            return EducatedGuess.Bool;

        var cur = EducatedGuess.Empty;
        foreach (var c in str)
        {
            if (c is ' ') continue;
            if ((c >= '0' && c <= '9') || c is '-')
            {
                Upgrade(ref cur, EducatedGuess.Integer);
            }
            else if (c is '.' or '-' or 'e' or 'E' or '+')
            {
                Upgrade(ref cur, EducatedGuess.Number);
            }
            else if (c is '{' or ',' or '}' or '(' or ')')
            {
                Upgrade(ref cur, EducatedGuess.TupleOf3Numbers);
            }
            else
            {
                return EducatedGuess.Unknown;
            }
        }

        return FinalCheck(cur, str);
    }

    private static EducatedGuess FinalCheck(EducatedGuess currentGuess, string str)
    {
        switch (currentGuess)
        {
            case EducatedGuess.Integer:
                if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    return EducatedGuess.Integer;
                break;
            case EducatedGuess.Number:
                if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    return EducatedGuess.Number;
                break;
            case EducatedGuess.TupleOf3Numbers:
                if (Point3dParser.IsValid(str))
                    return EducatedGuess.TupleOf3Numbers;
                break;
            default:
                return currentGuess;
        }

        return EducatedGuess.Unknown;
    }
    private static void Upgrade(ref EducatedGuess guess, EducatedGuess newVal)
    {
        if (newVal > guess)
            guess = newVal;
    }
    private static Parser? GuessType(Tree<string> tree)
    {
        EducatedGuess lastGuess = EducatedGuess.Unknown;

        foreach (var it in tree.AllPears)
        {
            if (it?.Item is not { } str)
                continue;

            var guess = GuessString(str);

            if (guess is EducatedGuess.Empty)
                continue;

            if (guess is EducatedGuess.Unknown)
                return null;

            if (guess == lastGuess)
                continue;

            if (lastGuess is EducatedGuess.Unknown)
            {
                lastGuess = guess;
                continue;
            }

            switch (guess)
            {
                case EducatedGuess.Bool:
                    return null;

                case EducatedGuess.Integer:
                    if (lastGuess is EducatedGuess.Number)
                        break;
                    return null;

                case EducatedGuess.Number:
                    if (lastGuess is EducatedGuess.Integer)
                    {
                        lastGuess = EducatedGuess.Number;
                        break;
                    }
                    else
                    {
                        return null;
                    }

                default:
                    return null;
            }
        }

        return lastGuess switch
        {
            EducatedGuess.Bool => BoolParser,
            EducatedGuess.Integer => IntParser,
            EducatedGuess.Number => DoubleParser,
            EducatedGuess.TupleOf3Numbers => Point3dParser,
            _ => null,
        };
    }

}
