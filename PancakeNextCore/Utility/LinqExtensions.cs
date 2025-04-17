using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal static class LinqExtensions
{
    public static IEnumerable<T> RecursivelyFlatten<T>(this IEnumerable<T> src, Func<T, IEnumerable<T>?> flattener)
    {
        foreach (var it in src)
        {
            yield return it;

            var inner = flattener(it);
            if (inner is null) continue;

            foreach (var innerIt in inner.RecursivelyFlatten(flattener))
            {
                yield return innerIt;
            }
        }
    }
}
