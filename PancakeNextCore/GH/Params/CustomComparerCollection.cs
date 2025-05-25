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
}
