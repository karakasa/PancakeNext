using Grasshopper2.Data;
using PancakeNextCore.GH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal sealed class PearEqualityComparer<T> : IEqualityComparer<Pear<T>>
    where T : IEquatable<T>
{
    public static readonly PearEqualityComparer<T> Instance = new();
    public bool Equals(Pear<T>? x, Pear<T>? y) => x.Item.Equals(y.Item);

    public int GetHashCode(Pear<T> obj) => obj.Item.GetHashCode();
}

internal sealed class PearEqualityComparerGeneric : IEqualityComparer<IPear>
{
    public static readonly PearEqualityComparerGeneric Instance = new();
    public bool Equals(IPear? x, IPear? y) => OptimizedOperators.SameContentQ(x, y);

    public int GetHashCode(IPear obj) => OptimizedOperators.Hashcode(obj);
}
internal sealed class PearComparerGeneric : IComparer<IPear>
{
    public static readonly PearComparerGeneric Instance = new();
    public int Compare(IPear? x, IPear? y) => OptimizedOperators.Compare(x, y);
}
