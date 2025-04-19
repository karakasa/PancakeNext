using Eto.Drawing;
using Grasshopper2.Types.Colour;
using PancakeNextCore.GH.Params;
using PancakeNextCore.GH.Params.AssocConverters;
using Rhino.Geometry;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Components.IO;
public sealed partial class pcParseString
{
    private bool TryParseXml(string strData, out GhAssocBase? result)
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

    private bool TryParseNull(string strData, out object result)
    {
        result = null;
        return strData == "null";
    }

    private bool TryParseCommaString(string strData, out string str)
    {
        if (strData.StartsWith("\"") && strData.EndsWith("\""))
        {
            str = strData.Substring(1, strData.Length - 2);
            return true;
        }
        str = default;
        return false;
    }
    internal static bool TryParsePoint(string strData, out Point3d point)
    {
        try
        {
            var blocks = strData.SplitWhile(StringUtility.IsNumericAndNegative).Select(s => s.Trim()).ToArray();
            if (blocks.Length == 5)
            {
                if (blocks[0] == "{" && blocks[4] == "}" && blocks[2] == ",")
                {
                    point = new Point3d(
                        Convert.ToDouble(blocks[1]),
                        Convert.ToDouble(blocks[3]),
                        0.0);
                    return true;
                }
            }
            else if (blocks.Length == 7)
            {
                if (blocks[0] == "{" && blocks[6] == "}" && blocks[2] == "," && blocks[4] == ",")
                {
                    point = new Point3d(
                        Convert.ToDouble(blocks[1]),
                        Convert.ToDouble(blocks[3]),
                        Convert.ToDouble(blocks[5]));
                    return true;
                }
            }
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

    internal static bool TryParseColor(string strData, out Colour color)
    {
        try
        {
            if ((strData.Length == 7 || strData.Length == 9) &&
                strData.StartsWith("#", StringComparison.Ordinal))
            {
                var part1 = int.Parse(strData.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                var part2 = int.Parse(strData.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                var part3 = int.Parse(strData.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                int part4;

                if (strData.Length == 9)
                {
                    part4 = int.Parse(strData.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
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
