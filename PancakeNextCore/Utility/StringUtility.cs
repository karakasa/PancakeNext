using PancakeNextCore.Utility;
using PancakeNextCore.Utility.Polyfill;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal static partial class StringUtility
{
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

    public static bool TrySplitWhile<TPredicate, TExecutor>(this string str, TPredicate predicate, ref TExecutor context)
        where TPredicate : struct, IStructPredicate<char>
        where TExecutor : struct, IStringPartExecutor
    {
        int lastResult = -1;
        var index = 0;

        var currentStartIndex = 0;
        var currentLength = 0;
        var partId = -1;

        foreach (var it in str)
        {
            var currentPredicate = predicate.Predicate(it) ? 1 : 0;
            if (lastResult == -1) lastResult = currentPredicate;
            if (currentPredicate != lastResult)
            {
                ++partId;
                if (!context.HandlePart(str, partId, currentStartIndex, currentLength)) return false;

                lastResult = currentPredicate;
                currentStartIndex = index;
                currentLength = 1;
            }
            else
            {
                ++currentLength;
            }

            ++index;
        }

        if (currentLength != 0)
        {
            ++partId;
            if (!context.HandlePart(str, partId, currentStartIndex, currentLength)) return false;
        }

        return true;
    }

    public static bool IsNumeric(char c) => (c >= '0' && c <= '9') || c == '.';

    public readonly struct IsNumericAndNegativePredicate : IStructPredicate<char>
    {
        public bool Predicate(char c) => IsNumericAndNegative(c);
    }
    public readonly struct IsNumericPredicate : IStructPredicate<char>
    {
        public bool Predicate(char c) => IsNumeric(c);
    }
    public static bool IsNumericAndNegative(char c) => (c >= '0' && c <= '9') || c == '.' || c == '-';

    public static bool IsNumeric(string s) => s.All<IsNumericPredicate>();
    public static void SplitLikelyOne(this string str, string[] separators, ref OptimizedConditionTester<string> result)
    {
        foreach (var it in separators)
        {
            if (str.Contains(it, StringComparison.Ordinal)) continue;

            result = new(str.Split(separators, StringSplitOptions.RemoveEmptyEntries));
            return;
        }

        result = string.IsNullOrEmpty(str) ? default : new(str);
    }
    public static bool All<TPredicate>(this string str) where TPredicate : struct, IStructPredicate<char>
    {
        foreach (var c in str)
        {
            if (!default(TPredicate).Predicate(c)) return false;
        }

        return true;
    }
}
