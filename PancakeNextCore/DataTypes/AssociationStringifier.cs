using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataType;
internal static class AssociationStringifier
{
    public static string ToString(object pancakeObj, StringConversionType style)
    {
        if (pancakeObj == null)
            return null;

        if (pancakeObj is GhAtomList list)
        {
            return ListToJson(list, style);
        }

        if (MetahopperWrapper.IsMetaHopperWrapper(pancakeObj))
        {
            try
            {
                var listObj = new GhAtomList(MetahopperWrapper.ExtractMetaHopperWrapper(pancakeObj));
                return ListToJson(listObj, style);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        if (pancakeObj is Association assoc)
        {
            return assoc.ToString(style);
        }

        return null;
    }

    private static readonly Dictionary<StringConversionType, string[]> StyleKeywords = new();
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
        switch (style)
        {
            case StringConversionType.Json:
                return EscapeStyle.Json;
            case StringConversionType.Association:
                return EscapeStyle.Mathematica;
            case StringConversionType.Python:
                return EscapeStyle.Python;
            default:
                throw new ArgumentOutOfRangeException(nameof(style));
        }
    }

    public string ToString(StringConversionType type)
    {
        switch (type)
        {
            case StringConversionType.Json:
                return ToStringJsonLike(this, StringConversionType.Json);
            case StringConversionType.Association:
                return ToStringJsonLike(this, StringConversionType.Association);
            case StringConversionType.Python:
                return ToStringJsonLike(this, StringConversionType.Python);
            case StringConversionType.AllStringJson:
                return ToStringAllString();
            case StringConversionType.Querystring:
                return ToStringQuery();
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }

    public static string ListToJson(GhAtomList obj, StringConversionType style)
    {
        return ToJsonLike(obj, style);
    }

    internal static string ToJsonLike(object obj, StringConversionType style)
    {
        if (obj is int || obj is double || obj is long || obj is float)
        {
            // Numeric type
            return obj.ToString();
        }

        if (obj is bool b)
        {
            // Boolean type
            return b ? StyleKeywords[style][Keyword.TrueVal]
                : StyleKeywords[style][Keyword.FalseVal];
        }

        if (obj is Association assoc)
            return ToStringJsonLike(assoc, style);

        if (obj is GhAtomList || MetahopperWrapper.IsMetaHopperWrapper(obj))
        {
            var atomlist = GhAtomList.CreateFromPossibleWrapper(obj);
            if (atomlist != null)
            {
                return StyleKeywords[style][Keyword.ArrayValL]
                    + string.Join(StyleKeywords[style][Keyword.ArrayComma],
                    atomlist.GetInnerListUnwrapped().Select(e => ToJsonLike(e, style)))
                    + StyleKeywords[style][Keyword.ArrayValR];
            }
        }

        if (obj == null)
        {
            return "null";
        }

        return StyleKeywords[style][Keyword.StrValL]
            + EscapeForJson(obj.ToString(), MapEscapeStyle(style))
            + StyleKeywords[style][Keyword.StrValR];
    }
    private static string ToStringJsonLike(Association assoc, StringConversionType style)
    {
        var innerStr = string.Join(StyleKeywords[style][Keyword.KVPairComma],
            assoc.GetJsonNames().Zip(assoc.Values, (name, value) =>
            string.Format(StyleKeywords[style][Keyword.KVPair], EscapeForJson(name), ToJsonLike(value, style))
            ));

        return StyleKeywords[style][Keyword.ObjValL]
            + innerStr
            + StyleKeywords[style][Keyword.ObjValR];
    }

    private string ToStringAllString()
    {
        var innerStr = string.Join(", ",
            GetJsonNames().Zip(Values, (name, value) =>
            $"\"{name}\": \"{EscapeForJson(value.ToString())}\""));

        return "{" + innerStr + "}";
    }

    private string ToStringQuery()
    {
        var innerStr = string.Join("&",
            GetNames().Zip(Values, (name, value) =>
            $"{name}={EscapeForUrl(value.ToString())}"));

        return innerStr;
    }
}
