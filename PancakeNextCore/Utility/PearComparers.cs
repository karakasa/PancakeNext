﻿using Grasshopper2.Data;
using GrasshopperIO.DataBase;
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
internal sealed class PearComparerGenericReversed : IComparer<IPear>
{
    public static readonly PearComparerGenericReversed Instance = new();
    public int Compare(IPear? x, IPear? y)
    {
        var result = -OptimizedOperators.Compare(x, y);
        return result;
    }
}
internal sealed class PearComparerNaturalSort : IComparer<IPear>, IComparer<string>
{
    public static readonly PearComparerNaturalSort Instance = new();
    public int Compare(IPear? x, IPear? y) => CompareString(x, y);
    public int Compare(string? x, string? y) => CompareString(x, y);
    public static int CompareString(IPear? x, IPear? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        var sx = GetString(x);
        var sy = GetString(y);

        return default(SimpleNaturalSort.Struct).Compare(sx, sy);
    }

    private static string GetString(IPear x)
    {
        if (x is Pear<string> pearStr) return pearStr.Item;
        return x.Item?.ToString() ?? "";
    }

    public static int CompareString(string? x, string? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        return default(SimpleNaturalSort.Struct).Compare(x, y);
    }
}
internal sealed class PearComparerNaturalSortReversed : IComparer<IPear>, IComparer<string>
{
    public static readonly PearComparerNaturalSortReversed Instance = new();
    public int Compare(IPear? x, IPear? y) => -PearComparerNaturalSort.CompareString(x, y);

    public int Compare(string? x, string? y) => -PearComparerNaturalSort.CompareString(x, y);
}
