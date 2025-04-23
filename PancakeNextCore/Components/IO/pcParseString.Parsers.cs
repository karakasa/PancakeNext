using Eto.Drawing;
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
using static Rhino.Runtime.ViewCaptureWriter;
using Path = System.IO.Path;

namespace PancakeNextCore.Components.IO;
public sealed partial class pcParseString
{
    private delegate bool TryParseDelegate<T>(string strData, [NotNullWhen(true)] out T result);
    private bool TryConvertGeneric<T>(string typeName, TryParseDelegate<T> parseFunc, string result,
        [NotNullWhen(true)] ref object? outObj, [NotNullWhen(true)] ref string? outTypeName)
    {
        if (!CheckDesire(typeName))
        {
            return false;
        }

        if (parseFunc(result, out var obj))
        {
            outObj = obj;
            outTypeName = typeName;
            return true;
        }

        return false;
    }

    private bool TryParseStringsAs<T>(string typeName, TryParseDelegate<T> parseFunc,
       Twig<string> strings, [NotNullWhen(true)] ref ITwig? outputs, [NotNullWhen(true)] ref string? outTypeName
       )
    {
        if (!CheckDesire(typeName))
        {
            return false;
        }

        var factory = new TwigFactory<T>(strings.LeafCount);

        foreach (var it in strings.Pears)
        {
            if (it is null)
            {
                factory.AddNull();
                continue;
            }

            var meta = it.Meta;
            if (parseFunc(it.Item, out var v))
            {
                factory.Add(v, meta, false);
            }
            else
            {
                factory.Add(default!, meta, true);
            }
        }

        outputs = factory.Create();
        outTypeName = typeName;
        return true;
    }

    private static Twig<T> ParseStringsAs<T>(TryParseDelegate<T> parseFunc, Twig<string> strings)
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
            if (parseFunc(it.Item, out var v))
            {
                factory.Add(v, meta, false);
            }
            else
            {
                factory.Add(default!, meta, true);
            }
        }

        return factory.Create();
    }

    private bool TryParseStringTreeAs<T>(string typeName, TryParseDelegate<T> parseFunc,
       Tree<string> strings, [NotNullWhen(true)] ref ITree? outputs, [NotNullWhen(true)] ref string? outTypeName
       )
    {
        if (!CheckDesire(typeName))
        {
            return false;
        }

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
                array[i] = ParseStringsAs(parseFunc, twig);
            }
        }

        outputs = Garden.TreeFromTwigs(strings.Paths, array);
        outTypeName = typeName;
        return true;
    }

    private void ParseStringTreeGeneric(Tree<string> strings, out ITree outputs, bool populateTypeNames,
        out Tree<string>? outTypeName)
    {
        var pathCount = strings.PathCount;
        var twigs = new ITwig[pathCount];

        var outputTypeNames = populateTypeNames ? new Twig<string>[pathCount] : null;

        for (var i = 0; i < pathCount; i++)
        {
            var strs = strings.Twigs[i];
            var j = 0;

            var cnt = strs.LeafCount;

            var twig = new IPear?[cnt];

            string[]? names = null;

            if (populateTypeNames)
            {
                names = new string[cnt];
            }

            foreach (var leaf in strs.Pears)
            {
                if (leaf is null || !ParseString(leaf.Item, out var to, out var typeName))
                {
                    to = null;
                    typeName = "?";
                }

                if (populateTypeNames)
                {
                    names[j] = typeName;
                }

                var pear = to.AsPear();
                if (leaf?.Meta is not null)
                {
                    pear = pear.WithMeta(leaf.Meta);
                }
                twig[j] = pear;

                ++j;
            }

            twigs[i] = Garden.ITwigFromPears(twig);
            if (populateTypeNames)
            {
                outputTypeNames[i] = Garden.TwigFromList(names);
            }
        }

        outputs = Garden.ITreeFromITwigs(twigs);

        if (populateTypeNames)
        {
            outTypeName = Garden.TreeFromTwigs(outputTypeNames);
        }
        else
        {
            outTypeName = null;
        }
    }

    private bool TryHandleFile(string path, [NotNullWhen(true)] ref object? outObj, [NotNullWhen(true)] ref string? outTypeName)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        string result;
        switch (extension)
        {
            case ".json":
                result = FileIo.ReadAllText(path);
                if (TryConvertGeneric<GhAssocBase>("Json", BuiltinJsonParser.TryParseJsonLight, result, ref outObj, ref outTypeName))
                    return true;
                break;
            case ".xml":
                result = FileIo.ReadAllText(path);
                if (TryConvertGeneric<GhAssocBase>("Xml", TryParseXml, result, ref outObj, ref outTypeName))
                    return true;
                break;
        }

        outObj = null;
        return false;
    }
    private bool TryParseStrings(Twig<string> from, [NotNullWhen(true)] out ITwig? to, [NotNullWhen(true)] out string? typeName)
    {
        typeName = null;
        to = null;

        if (
            TryParseStringsAs<GhAssocBase>("Json", BuiltinJsonParser.TryParseJsonLight, from, ref to, ref typeName) ||
            TryParseStringsAs<GhAssocBase>("Xml", TryParseXml, from, ref to, ref typeName) ||
            TryParseStringsAs<GhLengthDecimal>("DecimalLength", GhLengthDecimal.TryParse, from, ref to, ref typeName) ||
            TryParseStringsAs<GhLengthFeetInch>("FeetInchLength", GhLengthFeetInch.TryParse, from, ref to, ref typeName) ||
            TryParseStringsAs<Point3d>("Point", TryParsePoint, from, ref to, ref typeName) ||
            TryParseStringsAs<Interval>("Domain", TryParseInterval, from, ref to, ref typeName) ||
            TryParseStringsAs<Colour>("Colour", TryParseColor, from, ref to, ref typeName) ||
            TryParseStringsAs<GhAssocBase>("Xml", TryParseXml, from, ref to, ref typeName) ||
            TryParseStringsAs<object>("Null", TryParseNull, from, ref to, ref typeName) ||
            TryParseStringsAs<string>("CommaString", TryParseCommaString, from, ref to, ref typeName) ||
            TryParseStringsAs<int>("Integer", int.TryParse, from, ref to, ref typeName) ||
            TryParseStringsAs<double>("Number", double.TryParse, from, ref to, ref typeName) ||
            TryParseStringsAs<bool>("Boolean", bool.TryParse, from, ref to, ref typeName) ||
            TryParseStringsAs<DateTime>("DateTime", DateTime.TryParse, from, ref to, ref typeName) ||
            TryParseStringsAs<Guid>("Guid", Guid.TryParse, from, ref to, ref typeName)
            )
        {
            return true;
        }

        return false;
    }
    private bool TryParseStringTree(Tree<string> from, [NotNullWhen(true)] out ITree? to, [NotNullWhen(true)] out string? typeName)
    {
        typeName = null;
        to = null;

        if (
            TryParseStringTreeAs<GhAssocBase>("Json", BuiltinJsonParser.TryParseJsonLight, from, ref to, ref typeName) ||
            TryParseStringTreeAs<GhAssocBase>("Xml", TryParseXml, from, ref to, ref typeName) ||
            TryParseStringTreeAs<GhLengthDecimal>("DecimalLength", GhLengthDecimal.TryParse, from, ref to, ref typeName) ||
            TryParseStringTreeAs<GhLengthFeetInch>("FeetInchLength", GhLengthFeetInch.TryParse, from, ref to, ref typeName) ||
            TryParseStringTreeAs<Point3d>("Point", TryParsePoint, from, ref to, ref typeName) ||
            TryParseStringTreeAs<Interval>("Domain", TryParseInterval, from, ref to, ref typeName) ||
            TryParseStringTreeAs<Colour>("Colour", TryParseColor, from, ref to, ref typeName) ||
            TryParseStringTreeAs<GhAssocBase>("Xml", TryParseXml, from, ref to, ref typeName) ||
            TryParseStringTreeAs<object>("Null", TryParseNull, from, ref to, ref typeName) ||
            TryParseStringTreeAs<string>("CommaString", TryParseCommaString, from, ref to, ref typeName) ||
            TryParseStringTreeAs<int>("Integer", int.TryParse, from, ref to, ref typeName) ||
            TryParseStringTreeAs<double>("Number", double.TryParse, from, ref to, ref typeName) ||
            TryParseStringTreeAs<bool>("Boolean", bool.TryParse, from, ref to, ref typeName) ||
            TryParseStringTreeAs<DateTime>("DateTime", DateTime.TryParse, from, ref to, ref typeName) ||
            TryParseStringTreeAs<Guid>("Guid", Guid.TryParse, from, ref to, ref typeName)
            )
        {
            return true;
        }

        return false;
    }
    private bool ParseString(string from, [NotNullWhen(true)] out object? to, [NotNullWhen(true)] out string? typeName)
    {
        typeName = null;
        to = null;

        if (
            (FileIo.IsValidPath(from) && TryHandleFile(from, ref to, ref typeName)) ||
            TryConvertGeneric<GhAssocBase>("Json", BuiltinJsonParser.TryParseJsonLight, from, ref to, ref typeName) ||
            TryConvertGeneric<GhAssocBase>("Xml", TryParseXml, from, ref to, ref typeName) ||
            TryConvertGeneric<GhLengthDecimal>("DecimalLength", GhLengthDecimal.TryParse, from, ref to, ref typeName) ||
            TryConvertGeneric<GhLengthFeetInch>("FeetInchLength", GhLengthFeetInch.TryParse, from, ref to, ref typeName) ||
            TryConvertGeneric<Point3d>("Point", TryParsePoint, from, ref to, ref typeName) ||
            TryConvertGeneric<Interval>("Domain", TryParseInterval, from, ref to, ref typeName) ||
            TryConvertGeneric<Colour>("Colour", TryParseColor, from, ref to, ref typeName) ||
            TryConvertGeneric<GhAssocBase>("Xml", TryParseXml, from, ref to, ref typeName) ||
            TryConvertGeneric<object>("Null", TryParseNull, from, ref to, ref typeName) ||
            TryConvertGeneric<string>("CommaString", TryParseCommaString, from, ref to, ref typeName) ||
            TryConvertGeneric<int>("Integer", int.TryParse, from, ref to, ref typeName) ||
            TryConvertGeneric<double>("Number", double.TryParse, from, ref to, ref typeName) ||
            TryConvertGeneric<bool>("Boolean", bool.TryParse, from, ref to, ref typeName) ||
            TryConvertGeneric<DateTime>("DateTime", DateTime.TryParse, from, ref to, ref typeName) ||
            TryConvertGeneric<Guid>("Guid", Guid.TryParse, from, ref to, ref typeName)
            )
        {
            return true;
        }

        return false;
    }

    private bool TryParseXml(string strData, [NotNullWhen(true)] out GhAssocBase? result)
    {
        try
        {
            var assoc = XmlIo.ReadXml(strData, out _);
            if (assoc != null)
            {
                // _access?.AddWarning("Use XML to Assoc instead", Strings.ItSNotRecommendedToUseParseString);
            }
            result = assoc;
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    private bool TryParseNull(string strData, out object? result)
    {
        result = null;
        return strData == "null";
    }

    private bool TryParseCommaString(string strData, [NotNullWhen(true)] out string? str)
    {
        if (strData.StartsWith("\"") && strData.EndsWith("\""))
        {
            str = strData.Substring(1, strData.Length - 2);
            return true;
        }
        str = default;
        return false;
    }

    private struct ParsePointExecutor : IStringPartExecutor
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
                case 0 when length == 1:
                    return str[startIndex] == '{';
                case 1:
                    return str.TryParseTrimmedAsDouble(startIndex, length, out x);
                case 2 when length == 1:
                    return str[startIndex] == ',';
                case 3:
                    return str.TryParseTrimmedAsDouble(startIndex, length, out y);
                case 4 when length == 1:
                    switch (str[startIndex])
                    {
                        case '}':
                            threeDimensional = false;
                            return true;
                        case ',':
                            threeDimensional = true;
                            return true;
                        default:
                            return false;
                    }
                case 5 when threeDimensional:
                    return str.TryParseTrimmedAsDouble(startIndex, length, out z);
                case 6 when length == 1 && threeDimensional:
                    return str[startIndex] == '}';
                default:
                    return false;
            }
        }
    }

    internal static bool TryParsePoint(string strData, out Point3d point)
    {
        try
        {
            var executor = new ParsePointExecutor();

            if (!strData.TrySplitWhile(default(StringUtility.IsNumericAndNegativePredicate), ref executor))
            {
                point = default;
                return false;
            }

            point = executor.CollectResult();
            return true;
        }
        catch
        {

        }

        point = default;
        return false;
    }

    internal static bool TryParseInterval(string strData, out Interval domain)
    {
        try
        {
            var blocks = strData.SplitWhile(StringUtility.IsNumericAndNegative).Select(s => s.Trim()).ToArray();
            if (blocks.Length == 3)
                if (blocks[1] == "To" || blocks[1] == "~" || blocks[1] == "->")
                {
                    domain = new Interval(Convert.ToDouble(blocks[0]), Convert.ToDouble(blocks[2]));
                    return true;
                }
        }
        catch
        {

        }

        domain = default;
        return false;
    }

    internal static bool TryParseColor(string strData, [NotNullWhen(true)] out Colour? color)
    {
        try
        {
            if ((strData.Length == 7 || strData.Length == 9) &&
                strData.StartsWith("#", StringComparison.Ordinal))
            {
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
        catch
        {

        }

        color = default;
        return false;
    }
}
