using Grasshopper2.Data;
using Grasshopper2.Types.Colour;
using Grasshopper2.Types.Numeric;
using Grasshopper2.Types.Shapes;
using GrasshopperIO;
using PancakeNextCore.GH.Params;
using Rhino.Geometry;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH;
internal static class OptimizedOperators
{
    public static bool SameContentQ(IPear? a, IPear? b)
    {
        if (a is null || b is null || (a.Type != b.Type)) return false;
        if (object.ReferenceEquals(a, b)) return true;

        return FastSameContentQ(a, b) ?? Garden.PearEquality(a, b, false);
    }

    private static bool? FastSameContentQ(IPear a, IPear b)
    {
        var type = a.Type;
        if (type == typeof(int))
        {
            return FastSameContentQ(a as Pear<int>, b as Pear<int>);
        }
        else if (type == typeof(string))
        {
            return FastSameContentQ(a as Pear<string>, b as Pear<string>);
        }
        else if (type == typeof(double))
        {
            return FastSameContentQ(a as Pear<double>, b as Pear<double>);
        }
        else if (type == typeof(bool))
        {
            return FastSameContentQ(a as Pear<bool>, b as Pear<bool>);
        }
        else
        {
            return null;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool FastSameContentQ<T>(Pear<T>? a, Pear<T>? b) where T : IEquatable<T>
    {
        if (a is null || b is null) return false;
        return a.Item.Equals(b.Item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FastHashcode<T>(Pear<T>? a)
    {
        if (a is null) return 0;
        return a.Item.GetHashCode();
    }
    public static int Hashcode(IPear? a)
    {
        if (a is null) return 0;

        return FastHashcode(a) ?? a.Item?.GetHashCode() ?? 0;
    }

    private static int? FastHashcode(IPear a)
    {
        var type = a.Type;
        if (type == typeof(int))
        {
            return FastHashcode(a as Pear<int>);
        }
        else if (type == typeof(string))
        {
            return FastHashcode(a as Pear<string>);
        }
        else if (type == typeof(double))
        {
            return FastHashcode(a as Pear<double>);
        }
        else if (type == typeof(bool))
        {
            return FastHashcode(a as Pear<bool>);
        }
        else
        {
            return null;
        }
    }

    public static int Compare(IPear? a, IPear? b)
    {
        if (a is null || b is null || (a.Type != b.Type)) return Garden.PearComparison(a, b, false);
        return FastCompare(a, b) ?? Garden.PearComparison(a, b, false);
    }

    private static int? FastCompare(IPear a, IPear b)
    {
        var type = a.Type;
        if (type == typeof(int))
        {
            return FastCompare(a as Pear<int>, b as Pear<int>);
        }
        else if (type == typeof(string))
        {
            return FastCompare(a as Pear<string>, b as Pear<string>);
        }
        else if (type == typeof(double))
        {
            return FastCompare(a as Pear<double>, b as Pear<double>);
        }
        else if (type == typeof(bool))
        {
            return FastCompare(a as Pear<bool>, b as Pear<bool>);
        }
        else
        {
            return null;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FastCompare<T>(Pear<T>? a, Pear<T>? b) where T : IComparable<T>
    {
        if (a is null || b is null) return 0;
        return a.Item.CompareTo(b.Item);
    }

    public static IEnumerable<IPear?> AsPears(this IEnumerable<object?> objs)
        => objs.Select(AsPear);
    public static IPear? AsPear(this object? obj)
    {
        return obj switch
        {
            null => null,
            IPear v => v,
            bool v => Garden.Pear(v),
            byte v => Garden.Pear(v),
            sbyte v => Garden.Pear(v),
            int v => Garden.Pear(v),
            long v => Garden.Pear(v),
            uint v => Garden.Pear(v),
            ulong v => Garden.Pear(v),
            Guid v => Garden.Pear(v),
            BigInteger v => Garden.Pear(v),
            DateTime v => Garden.Pear(v),
            TimeSpan v => Garden.Pear(v),
            float v => Garden.Pear(v),
            double v => Garden.Pear(v),
            decimal v => Garden.Pear(v),
            Complex v => Garden.Pear(v),
            string v => Garden.Pear(v),
            AbsRelPaths v => Garden.Pear(v),
            Version v => Garden.Pear(v),
            Colour v => Garden.Pear(v),
            Angle v => Garden.Pear(v),
            Interval v => Garden.Pear(v),
            Grasshopper2.Data.Path v => Garden.Pear(v),
            Transform v => Garden.Pear(v),
            GhAssocBase v => Garden.Pear(v),
            GhQuantity v => Garden.Pear(v),
            _ => AsPearRare(obj)
        };
    }

    private static IPear? AsPearRare(object obj)
    {
        if (obj is ValueType)
        {
            return Garden.IPear(obj);
        }
        else
        {
            return Garden.Pear(obj);
        }
    }
}
