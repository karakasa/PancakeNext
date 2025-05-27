using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
public sealed class CustomComparerCollection : List<CustomComparer>, ICustomComparer
{
    int ICustomComparer.Count => Count;

    CustomComparer ICustomComparer.GetAt(int index)
    {
        if (index < 0) return this[0];
        if (index >= Count) return this[Count - 1];
        return this[index];
    }
    public override string ToString()
    {
        if (Count == 0) return "Invalid comparer collection";
        if (Count == 1) return this[0]?.ToString() ?? "";
        return $"Chained {Count} comparers";
    }
}
