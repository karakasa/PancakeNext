using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility.Polyfill;
internal static class StringSpanOperations
{
    public static bool Equals(string? a, string? b) => a == b;
#if NET
    public static bool Equals(ReadOnlySpan<char> a, string? b) => a.SequenceEqual(b);
    public static bool Equals(string? a, ReadOnlySpan<char> b) => Equals(b, a);
    public static bool Equals(ReadOnlySpan<char> a, ReadOnlySpan<char> b) => a.SequenceEqual(b);
#endif

    public static bool TryParseSubstrAsInt(this string val, int startIndex, out int result)
    {
#if NET
        return int.TryParse(val.AsSpan(startIndex), out result);
#else
        return int.TryParse(val.Substring(startIndex), out result);
#endif
    }

    public static bool TryParseSubstrAsInt(this string val, int startIndex, int length, out int result, NumberStyles styles)
    {
#if NET
        return int.TryParse(val.AsSpan(startIndex, length), styles, CultureInfo.InvariantCulture, out result);
#else
        return int.TryParse(val.Substring(startIndex, length), styles, CultureInfo.InvariantCulture, out result);
#endif
    }

    public static bool TryParseSubstrAsDouble(this string val, int startIndex, int length, out double result)
    {
#if NET
        return double.TryParse(val.AsSpan(startIndex, length), out result);
#else
        return double.TryParse(val.Substring(startIndex, length), out result);
#endif
    }

#if !NET
    public static string Substr(this string val, int startIndex, int length)
    {
        return val.Substring(startIndex, length);
    }
#else
    public static ReadOnlySpan<char> Substr(this string val, int startIndex, int length)
    {
        return val.AsSpan(startIndex, length);
    }
#endif

    public static bool EqualsSubstring(string a, int startIndexInA, int lengthInA, string b)
    {
#if NET
        return Equals(a.AsSpan(startIndexInA, lengthInA), b);
#else
        return Equals(a.Substring(startIndexInA, lengthInA), b);
#endif
    }

    public static bool TryParseTrimmedAsDouble(this string a, int startIndex, int length, out double v)
    {
        return double.TryParse(a.Substr(startIndex, length).Trim(), out v);
    }

#if !NET
    public static bool Contains(this string a, string b, StringComparison cond)
    {
        return a.IndexOf(b, cond) >= 0;
    }
#endif
}
