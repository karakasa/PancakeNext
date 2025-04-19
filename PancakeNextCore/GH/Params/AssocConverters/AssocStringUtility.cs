using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace PancakeNextCore.GH.Params.AssocConverters;

internal static partial class AssocStringUtility
{
    public static string EscapeForXml(this string str)
    {
        var sb = _sharedBuilder.Value;
        sb.Clear();
        str.EscapeForXml(sb);
        return sb.ToString();
    }

    public static void EscapeForXml(this string? str, StringBuilder sb)
    {
        if (str is null) return;

        foreach (var c in str)
        {
            var it = c switch
            {
                '"' => "&quot;",
                '\'' => "&apos;",
                '&' => "&amp;",
                '<' => "&lt;",
                '>' => "&gt;",
                _ => null
            };

            if (it is null)
                sb.Append(c);
            else
                sb.Append(it);
        }
    }

    private static ThreadLocal<StringBuilder> _sharedBuilder = new(static () => new(), false);

    public static string EscapeForJson(string raw, EscapeStyle style = EscapeStyle.Json)
    {
        var sb = _sharedBuilder.Value;
        sb.Clear();
        EscapeCharByChar(raw, style, sb);
        return sb.ToString();
    }

    private static void EscapeCharByChar(string raw, EscapeStyle style, StringBuilder sb)
    {
        var si = new StringInfo(raw);
        var siLength = si.LengthInTextElements;

        for (var i = 0; i < siLength; i++)
        {
            var it = si.SubstringByTextElements(i, 1);
            var it2 = it switch
            {
                "\n" => "\\n",
                "\r" => "\\r",
                "\t" => "\\t",
                "\\" => "\\\\",
                "\"" => "\\\"",
                "\b" => "\\b",
                "\f" => "\\f",
                "/" when style == EscapeStyle.Json => "\\/",
                "'" when style == EscapeStyle.Python => "\\'",
                _ => it,
            };
            sb.Append(it2);
        }
    }

    public static char UnescapeJsonSpecialChar(char c)
    {
        return c switch
        {
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            '\\' => '\\',
            '"' => '"',
            '/' => '/',
            'b' => '\b',
            'f' => '\f',
            _ => char.MinValue,
        };
    }

    public static string EscapeForUrl(string raw)
    {
        return raw;
    }

    private enum NumericRecognizeResult
    {
        Undefined,
        Integer,
        Decimal,
        Nonnumeric
    }

    private static NumericRecognizeResult RecognizeNumericFormat(string strData)
    {
        var result = NumericRecognizeResult.Integer;
        var dotExist = false;

        var str = strData.Trim();

        if (str.Length == 0) return NumericRecognizeResult.Nonnumeric;

        var skipFirst = str[0] == '-';

        foreach (var c in str)
        {
            if (skipFirst)
            {
                skipFirst = false;
                continue;
            }

            if (c == '.')
            {
                if (!dotExist)
                    return NumericRecognizeResult.Nonnumeric;
                result = NumericRecognizeResult.Decimal;
                dotExist = true;
                continue;
            }

            if (c < '0' || c > '9')
                return NumericRecognizeResult.Nonnumeric;
        }

        return result;
    }

    private static bool RecognizeBooleanFormat(string strData, ref bool result)
    {
        if (strData.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }

        if (strData.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }

        return false;
    }

    public static object RecognizeType(string strData)
    {
        var str = strData.Trim();
        if (str.StartsWith("\"") && str.EndsWith("\""))
        {
            str = str.Substring(1, str.Length - 2);
        }

        switch (RecognizeNumericFormat(str))
        {
            case NumericRecognizeResult.Integer:
                if (int.TryParse(str, out var integer))
                    return integer;
                else
                    return str;
            case NumericRecognizeResult.Decimal:
                if (double.TryParse(str, out var dbl))
                    return dbl;
                else
                    return str;
        }

        var result = false;
        if (RecognizeBooleanFormat(str, ref result))
            return result;

        return str;
    }

    public static IEnumerable<Guid> ToGuids(this IEnumerable<string> strs) => strs.Select(str => new Guid(str));
}
