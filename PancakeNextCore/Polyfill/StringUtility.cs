using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Polyfill;
internal static class StringUtility
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

    public static IEnumerable<string> SplitWhile(this string str, Func<char, bool> predicate)
    {
        int lastResult = -1;
        var currentStr = string.Empty;

        foreach (var it in str)
        {
            var currentPredicate = predicate(it) ? 1 : 0;
            if (lastResult == -1) lastResult = currentPredicate;
            if (currentPredicate != lastResult)
            {
                yield return currentStr;
                lastResult = currentPredicate;
                currentStr = "" + it;
            }
            else
            {
                currentStr += it;
            }
        }

        if (currentStr != string.Empty)
        {
            yield return currentStr;
        }
    }

    public static bool IsNumeric(char c) => (c >= '0' && c <= '9') || c == '.';

    public static bool IsNumericAndNegative(char c) => (c >= '0' && c <= '9') || c == '.' || c == '-';

    public static bool IsNumeric(string s) => s.All(IsNumeric);
}
