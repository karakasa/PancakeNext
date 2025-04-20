using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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
        return TryParseSubstrAsInt(val, startIndex, out result, NumberStyles.Integer);
    }

    public static bool TryParseSubstrAsInt(this string val, int startIndex, out int result, NumberStyles styles)
    {
        return int.TryParse(val.Substr(startIndex), styles, CultureInfo.InvariantCulture, out result);
    }

    public static bool TryParseSubstrAsInt(this string val, int startIndex, int length, out int result, NumberStyles styles)
    {
        return int.TryParse(val.Substr(startIndex, length), styles, CultureInfo.InvariantCulture, out result);
    }

    public static bool TryParseSubstrAsDouble(this string val, int startIndex, int length, out double result)
    {
        return TryParseSubstrAsDouble(val, startIndex, length, out result, NumberStyles.Float);
    }

    public static bool TryParseSubstrAsDouble(this string val, int startIndex, int length, out double result, NumberStyles styles)
    {
        return double.TryParse(val.Substr(startIndex, length), styles, CultureInfo.InvariantCulture, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !NET
    public static string Substr(this string val, int startIndex)
    {
        return val.Substring(startIndex);
    }
#else
    public static ReadOnlySpan<char> Substr(this string val, int startIndex)
    {
        return val.AsSpan(startIndex);
    }
#endif

    public static bool EqualsSubstring(string a, int startIndexInA, int lengthInA, string b)
    {
        return Equals(a.Substr(startIndexInA, lengthInA), b);
    }

    public static bool TryParseTrimmedAsDouble(this string a, int startIndex, int length, out double v)
    {
        const NumberStyles style = NumberStyles.Integer | NumberStyles.AllowDecimalPoint;
        return double.TryParse(a.Substr(startIndex, length), style, CultureInfo.InvariantCulture, out v);
    }

#if !NET
    public static bool Contains(this string a, string b, StringComparison cond)
    {
        return a.IndexOf(b, cond) >= 0;
    }
#endif
}
