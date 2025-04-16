using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Helper;
internal static class ParameterSizeEstimator
{
    public static ulong Estimate(IParameter p)
    {
        if (p.Inputs.Count > 0) return 0;

        ulong sum = 0;

        foreach (var it in p.PersistentDataWeak.AllItems)
        {
            sum += EstimateType(it);
        }

        return sum;
    }

    private static class BuiltinSizes
    {
        const ulong Float = sizeof(float);
        const ulong Double = sizeof(double);

        public const ulong Point3f = 3 * Float;
        public const ulong Point2f = 2 * Double;
        public const ulong Point3d = 3 * Double;
        public const ulong Point2d = 2 * Double;
        public const ulong Interval = Point2d;
        public const ulong Plane = 4 * Point3d;
        public const ulong BoundingBox = 2 * Point3d;
        public const ulong Arc = Plane + Interval + Double;
        public const ulong Box = Plane + 3 * Interval;
        public const ulong Circle = Plane + Double;
        public const ulong Sphere = Plane + Double;
        public const ulong Rectangle3d = Plane + Interval * 2;
    }

    private static ulong EstimateType(object? obj)
    {
        return obj switch
        {
            null => 0,
            string str => (ulong)(str.Length * 2),
            sbyte or byte or bool => 1,
            short or ushort or char => 2,
            int or uint or float => 4,
            ulong or long or double => 8,
            decimal => 16,
            Point3f or Vector3f => BuiltinSizes.Point3f,
            Point2f or Vector2f => BuiltinSizes.Point2f,
            Point3d or Vector3d => BuiltinSizes.Point3d,
            Point2d or Vector2d or Interval => BuiltinSizes.Point2d,
            Plane => BuiltinSizes.Plane,
            BoundingBox or Line => BuiltinSizes.BoundingBox,
            Arc => BuiltinSizes.Arc,
            Box => BuiltinSizes.Box,
            Circle => BuiltinSizes.Circle,
            Sphere => BuiltinSizes.Sphere,
            Rectangle3d => BuiltinSizes.Rectangle3d,
            System.Numerics.Complex => 2 * sizeof(double),
            GeometryBase geo => geo.MemoryEstimate(),
            Array array => EstimateArray(array),
            ValueType => EstimateValueType(obj.GetType()),
            _ => 0,
        };
    }

    private static readonly Dictionary<Type, ulong> ValueTypeSize = [];
    private static ulong EstimateValueType(Type type)
    {
        if (ValueTypeSize.TryGetValue(type, out var v)) return v;
        return ValueTypeSize[type] = (ulong)Marshal.SizeOf(type); // Inaccurate, but an estimate.
    }
    private static ulong EstimateArray(Array array)
    {
        if (array.Length == 0) return 0;
        var elemType = array.GetType().GetElementType();
        if (elemType is null) return 0;

        if (elemType.IsValueType)
        {
            return (ulong)array.Length * EstimateType(array.GetValue(0));
        }
        else
        {
            ulong sum = 0;
            foreach (var it in array)
            {
                sum += EstimateType(it);
            }
            return sum;
        }
    }

    public static ulong Estimate(Component comp)
    {
        ulong sum = 0;

        foreach (var p in comp.Parameters.Inputs)
        {
            sum += Estimate(p);
        }

        return sum;
    }
}
