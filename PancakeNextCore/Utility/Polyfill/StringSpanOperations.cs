using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
#if NET
        return Equals(a.AsSpan(startIndexInA, lengthInA), b);
#else
        if (b.Length != lengthInA) return false;
        foreach (var c in b)
        {
            if (a[startIndexInA++] != c) return false;
        }

        return true;
#endif
    }

    public static bool TryParseTrimmedAsDouble(this string a, int startIndex, int length, out double v)
    {
        const NumberStyles style = NumberStyles.Float;
        return double.TryParse(a.Substr(startIndex, length), style, CultureInfo.InvariantCulture, out v);
    }

    public static bool TryParseTrimmedAsInt(this string a, int startIndex, int length, out int v)
    {
        const NumberStyles style = NumberStyles.Integer;
        return int.TryParse(a.Substr(startIndex, length), style, CultureInfo.InvariantCulture, out v);
    }

#if !NET
    public static bool Contains(this string a, string b, StringComparison cond)
    {
        return a.IndexOf(b, cond) >= 0;
    }
#endif

    public static bool TrimmedEquals(this string a, int start, int length, string expected)
    {
#if NET
        return a.AsSpan(start, length).Trim() == expected;
#else
        if (length == expected.Length) return a == expected;
        const char Blank = ' ';

        var lastPos = Math.Min(start + length, a.Length - 1);

        for (; start <= lastPos; ++start)
        {
            if (a[start] != Blank) break;
        }

        for (; lastPos >= start; --lastPos)
        {
            if (a[lastPos] != Blank) break;
        }

        if (lastPos < start) return false;

        return EqualsSubstring(a, start, lastPos - start + 1, expected);
#endif
    }

    public static void TrimToPosition(this string a, ref int start, ref int length)
    {
        const char Blank = ' ';

        var lastPos = Math.Min(start + length, a.Length - 1);

        for (; start <= lastPos; ++start)
        {
            if (a[start] != Blank) break;
        }

        for (; lastPos >= start; --lastPos)
        {
            if (a[lastPos] != Blank) break;
        }

        length = lastPos - start + 1;
    }

    public static bool TrimmedEquals(this string a, int start, int length, char expected)
    {
#if NET
        var span = a.AsSpan(start, length).Trim();
        return span.Length == 1 && span[0] == expected;
#else
        if (length == 1) return a[0] == expected;

        const char Blank = ' ';

        var lastPos = Math.Min(start + length, a.Length - 1);

        for (; start <= lastPos; ++start)
        {
            var c = a[start];
            if (c == Blank) continue;
            if (c != expected) return false;
            for (++start; start <= lastPos; ++start)
            {
                if (a[start] != Blank) return false;
            }

            return true;
        }

        return false;
#endif
    }

    public static bool TrimmedEquals(this string a, int start, int length, char e1, char e2)
    {
        char cc;
#if NET
        var span = a.AsSpan(start, length).Trim();
        return span.Length == 1 && ((cc = span[0]) == e1 || cc == e2);
#else
        
        if (length == 1) return (cc = a[0]) == e1 || cc == e2;

        const char Blank = ' ';

        var lastPos = Math.Min(start + length, a.Length - 1);

        for (; start <= lastPos; ++start)
        {
            var c = a[start];
            if (c == Blank) continue;
            if (c != e1 && c != e2) return false;
            for (++start; start <= lastPos; ++start)
            {
                if (a[start] != Blank) return false;
            }

            return true;
        }

        return false;
#endif
    }

    public static int TrimmedEquals(this string a, int start, int length, char e1, char e2, char e3)
    {
        char cc;
#if NET
        var span = a.AsSpan(start, length).Trim();
        if (span.Length != 1) return -1;

        cc = span[0];
        if (cc == e1) return 0;
        if (cc == e2) return 1;
        if (cc == e3) return 2;

        return -1;
#else
        
                if (length == 1)
        {
            cc = a[0];
            if (cc == e1) return 0;
            if (cc == e2) return 1;
            if (cc == e3) return 2;

            return -1;
        }

        const char Blank = ' ';

        var lastPos = Math.Min(start + length, a.Length - 1);

        for (; start <= lastPos; ++start)
        {
            var c = a[start];
            if (c == Blank) continue;
            for (++start; start <= lastPos; ++start)
            {
                if (a[start] != Blank) return -1;
            }

            if (c == e1) return 0;
            if (c == e2) return 1;
            if (c == e3) return 2;
            return -1;
        }

        return -1;
#endif
    }
}
