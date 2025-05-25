using Grasshopper2.Data;
using PancakeNextCore.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
public enum ComparerType
{
    Default,
    BuiltinNaturalSort,
    Custom
}
public sealed class CustomComparer : ICustomComparer
{
    public ComparerType Type { get; set; } = ComparerType.Default;
    public bool OriginalOrder { get; set; } = true;

    int ICustomComparer.Count => 1;

    CustomComparer ICustomComparer.GetAt(int index) => this;
    public IComparer<IPear>? CustomPear { get; private set; }
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
            default:
                throw new ArgumentOutOfRangeException(nameof(Type), Type, null);
        }
    }

    private sealed class ReverselyOrderedComparer(IComparer<IPear> comparer) : IComparer<IPear>
    {
        readonly IComparer<IPear> _comparer = comparer;
        public int Compare(IPear? x, IPear? y) => 1 - _comparer.Compare(x, y);
    }
}
