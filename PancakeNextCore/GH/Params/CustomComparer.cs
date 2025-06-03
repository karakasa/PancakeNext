using Grasshopper2.Data;
using PancakeNextCore.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace PancakeNextCore.GH.Params;
public enum ComparerType
{
    Default,
    BuiltinNaturalSort,
    Custom,
    CustomNative
}
public sealed class CustomComparer : ICustomComparer
{
    internal int BuiltinId { get; private set; } = -1;
    public CustomComparer() { }
    internal CustomComparer(int builtinId, ComparerType type, bool originalOrder)
    {
        BuiltinId = builtinId;
        Type = type;
        OriginalOrder = originalOrder;
    }

    public CustomComparer(IComparer<IPear> pear, bool originalOrder)
    {
        CustomPear = pear ?? throw new ArgumentNullException(nameof(pear), "Custom comparer cannot be null.");
        Type = ComparerType.Custom;
        OriginalOrder = originalOrder;
    }

    public CustomComparer(IComparer native, bool originalOrder)
    {
        CustomNative = native ?? throw new ArgumentNullException(nameof(native), "Custom comparer cannot be null.");
        Type = ComparerType.CustomNative;
        OriginalOrder = originalOrder;
    }

    internal static readonly CustomComparer DefaultAscending = new(0, ComparerType.Default, true);
    internal static readonly CustomComparer DefaultDescending = new(1, ComparerType.Default, false);
    internal static readonly CustomComparer NaturalAscending = new(2, ComparerType.BuiltinNaturalSort, true);
    internal static readonly CustomComparer NaturalDescending = new(3, ComparerType.BuiltinNaturalSort, false);

    internal static CustomComparer ByBuiltInId(int id) => id switch
    {
        0 => DefaultAscending,
        1 => DefaultDescending,
        2 => NaturalAscending,
        3 => NaturalDescending,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Invalid builtin comparer id.")
    };
    public static CustomComparer ByBuiltIn(ComparerType type, bool reversed = false)
    {
        return type switch
        {
            ComparerType.Default => reversed ? DefaultDescending : DefaultAscending,
            ComparerType.BuiltinNaturalSort => reversed ? NaturalDescending : NaturalAscending,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid comparer type.")
        };
    }
    public static CustomComparer ByPearComparer(IComparer<IPear> comparer, bool reversed = false)
        => new() { CustomPear = comparer, Type = ComparerType.Custom, OriginalOrder = !reversed };
    public static CustomComparer ByNativeComparer(IComparer comparer, bool reversed = false)
        => new() { CustomNative = comparer, Type = ComparerType.CustomNative, OriginalOrder = !reversed };
    public ComparerType Type { get; set; } = ComparerType.Default;
    public bool OriginalOrder { get; set; } = true;
    int ICustomComparer.Count => 1;
    CustomComparer ICustomComparer.GetAt(int index) => this;
    public IComparer<IPear>? CustomPear { get; private set; }
    public IComparer? CustomNative { get; private set; }
    public IComparer<IPear> GetComparer()
    {
        switch (Type)
        {
            case ComparerType.Default:
                return OriginalOrder ? PearComparerGeneric.Instance : PearComparerGenericReversed.Instance;

            case ComparerType.BuiltinNaturalSort:
                return OriginalOrder ? PearComparerNaturalSort.Instance : PearComparerNaturalSortReversed.Instance;

            case ComparerType.Custom:
                if (CustomPear is null) throw new InvalidOperationException("Custom comparer is not set.");
                return OriginalOrder ? CustomPear : new ReverselyOrderedComparer(CustomPear);

            case ComparerType.CustomNative:
                if (CustomNative is null) throw new InvalidOperationException("Custom comparer is not set.");
                return new CustomNativeWrapper(CustomNative, !OriginalOrder);

            default:
                throw new ArgumentOutOfRangeException(nameof(Type), Type, null);
        }
    }
    public IComparer<T> GetStronglyTypedComparer<T>()
    {
        switch (Type)
        {
            case ComparerType.Default:
                return OriginalOrder ? Comparer<T>.Default : new ReverselyOrderedNativeComparer<T>(Comparer<T>.Default);

            case ComparerType.BuiltinNaturalSort:
                if (typeof(T) != typeof(string)) throw new InvalidOperationException($"Natural sort cannot be applied to type {typeof(T).FullName}");
                return (IComparer<T>)(object)(OriginalOrder ? PearComparerNaturalSort.Instance : PearComparerNaturalSortReversed.Instance);

            case ComparerType.Custom:
                if (CustomPear is not IComparer<Pear<T>> comp2) throw new InvalidOperationException(CustomPear is null ? "Custom comparer is not set." : $"Custom comparer is not of type IComparer<Pear<{typeof(T).Name}>>.");
                return new CustomPearWrapper<T>(comp2, OriginalOrder);

            case ComparerType.CustomNative:
                if (CustomNative is not IComparer<T> comp) throw new InvalidOperationException(CustomNative is null ? "Custom comparer is not set." : $"Custom comparer is not of type IComparer<{typeof(T).Name}>.");
                return OriginalOrder ? comp : new ReverselyOrderedNativeComparer<T>(comp);

            default:
                throw new ArgumentOutOfRangeException(nameof(Type), Type, null);
        }
    }
    private sealed class ReverselyOrderedNativeComparer<T>(IComparer<T> comparer) : IComparer<T>
    {
        readonly IComparer<T> _comparer = comparer;
        public int Compare(T x, T y) => -_comparer.Compare(x, y);
    }
    private sealed class ReverselyOrderedComparer(IComparer<IPear> comparer) : IComparer<IPear>
    {
        readonly IComparer<IPear> _comparer = comparer;
        public int Compare(IPear? x, IPear? y) => -_comparer.Compare(x, y);
    }
    private sealed class CustomNativeWrapper(IComparer comparer, bool reverse) : IComparer<IPear>
    {
        readonly IComparer _comparer = comparer;
        readonly bool _reverse = reverse;
        public int Compare(IPear? x, IPear? y)
        {
            var result = _comparer.Compare(x?.Item, y?.Item);
            if (_reverse) result = -result;
            return result;
        }
    }
    private sealed class CustomPearWrapper<T>(IComparer<Pear<T>> comparer, bool reverse) : IComparer<T>
    {
        readonly IComparer<Pear<T>> _comparer = comparer;
        readonly bool _reverse = reverse;
        public int Compare(T x, T y)
        {
            var result = _comparer.Compare(Garden.Pear(x), Garden.Pear(y));
            if (_reverse) result = -result;
            return result;
        }
    }

    public override string ToString()
    {
        return Type switch
        {
            ComparerType.Default => $"Pancake default comparer" + (OriginalOrder ? "" : " (descending)"),
            ComparerType.BuiltinNaturalSort => $"Natural sort comparer" + (OriginalOrder ? "" : " (descending)"),
            ComparerType.Custom => $"Custom comparer ({CustomPear?.GetType().Name ?? "null"})" + (OriginalOrder ? "" : " (reversed)"),
            ComparerType.CustomNative => $"Custom CLR comparer ({CustomNative?.GetType().Name ?? "null"})" + (OriginalOrder ? "" : " (reversed)"),
            _ => "Invalid comparer"
        };
    }
}
