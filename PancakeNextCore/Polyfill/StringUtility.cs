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
}
