using Eto.Drawing;
using Grasshopper2.Types.Colour;
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

namespace PancakeNextCore.Components.IO;
public sealed partial class pcParseString
{
    private bool TryParseXml(string strData, [NotNullWhen(true)] out GhAssocBase? result)
    {
        try
        {
            var assoc = XmlIo.ReadXml(_result, out _);
            if (assoc != null)
            {
                _access?.AddWarning("Use XML to Assoc instead", Strings.ItSNotRecommendedToUseParseString);
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
