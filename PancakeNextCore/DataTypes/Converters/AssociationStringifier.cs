using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataTypes.Converters;

internal static class AssociationStringifier
{
    public static string? ToString(object pancakeObj, StringConversionType style)
    {
        if (pancakeObj == null)
            return null;

        if (pancakeObj is AtomList list)
        {
            return ListToJson(list, style);
        }

        if (pancakeObj is Association assoc)
        {
            return AssocToString(assoc, style);
        }

        return null;
    }

    private static readonly Dictionary<StringConversionType, string[]> StyleKeywords = [];
    public enum StringConversionType
    {
        Json,
        AllStringJson,
        Querystring,
        Association,
        Python
    }

    private static class Keyword
    {
        public const int TrueVal = 0;
        public const int FalseVal = 1;
        public const int ArrayValL = 2;
        public const int ArrayValR = 3;
        public const int StrValL = 4;
        public const int StrValR = 5;
        public const int ObjValL = 6;
        public const int ObjValR = 7;
        public const int ArrayComma = 8;
        public const int KVPairComma = 9;
        public const int KVPair = 10;
    }

    private static EscapeStyle MapEscapeStyle(StringConversionType style)
    {
        return style switch
        {
            StringConversionType.Json => EscapeStyle.Json,
            StringConversionType.Association => EscapeStyle.Wolfram,
            StringConversionType.Python => EscapeStyle.Python,
            _ => throw new ArgumentOutOfRangeException(nameof(style)),
        };
    }

    public static string AssocToString(Association? assoc, StringConversionType type)
    {
        if (assoc is null) return "null";

        return type switch
        {
            StringConversionType.Json => ToStringJsonLike(assoc, StringConversionType.Json),
            StringConversionType.Association => ToStringJsonLike(assoc, StringConversionType.Association),
            StringConversionType.Python => ToStringJsonLike(assoc, StringConversionType.Python),
            StringConversionType.AllStringJson => ToStringAllString(assoc),
            StringConversionType.Querystring => ToStringQuery(assoc),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }

    public static string ListToJson(AtomList list, StringConversionType style)
    {
        if (list.HasValues)
        {
            return StyleKeywords[style][Keyword.ArrayValL]
            + string.Join(StyleKeywords[style][Keyword.ArrayComma],
            list.Values.Select(e => ToJsonLike(e, style)))
            + StyleKeywords[style][Keyword.ArrayValR];
        }
        else
        {
            return StyleKeywords[style][Keyword.ArrayValL] + StyleKeywords[style][Keyword.ArrayValR];
        }
    }

    internal static string ToJsonLike(object? obj, StringConversionType style)
    {
        if (obj == null)
        {
            return "null";
        }

        if (obj is int || obj is double || obj is long || obj is float)
        {
            // Numeric type
            return obj.ToString()!;
        }

        if (obj is bool b)
        {
            // Boolean type
            return b ? StyleKeywords[style][Keyword.TrueVal]
                : StyleKeywords[style][Keyword.FalseVal];
        }

        if (obj is Association assoc)
            return ToStringJsonLike(assoc, style);

        if (obj is AtomList list)
        {
            return ListToJson(list, style);
        }

        return StyleKeywords[style][Keyword.StrValL]
            + StringUtility.EscapeForJson(obj?.ToString() ?? "null", MapEscapeStyle(style))
            + StyleKeywords[style][Keyword.StrValR];
    }
    private static string ToStringJsonLike(Association assoc, StringConversionType style)
    {
        if (!assoc.HasValues)
        {
            return StyleKeywords[style][Keyword.ObjValL] + StyleKeywords[style][Keyword.ObjValR];
        }

        var innerStr = string.Join(StyleKeywords[style][Keyword.KVPairComma],
            assoc.GetNamesForExport().Zip(assoc.Values, (name, value) =>
            string.Format(StyleKeywords[style][Keyword.KVPair], StringUtility.EscapeForJson(name), ToJsonLike(value, style))
            ));

        return StyleKeywords[style][Keyword.ObjValL]
            + innerStr
            + StyleKeywords[style][Keyword.ObjValR];
    }

    private static string ToStringAllString(Association assoc)
    {
        if (!assoc.HasValues) return "{}";

        var innerStr = string.Join(", ",
            assoc.GetNamesForExport().Zip(assoc.Values, (name, value) =>
            $"\"{name}\": \"{StringUtility.EscapeForJson(value?.ToString() ?? "null")}\""));

        return "{" + innerStr + "}";
    }

    private static string ToStringQuery(Association assoc)
    {
        if (!assoc.HasValues) return "";

        var innerStr = string.Join("&",
            assoc.GetNamesForExport().Zip(assoc.Values, (name, value) =>
            $"{name}={StringUtility.EscapeForUrl(value?.ToString() ?? "null")}"));

        return innerStr;
    }
}
