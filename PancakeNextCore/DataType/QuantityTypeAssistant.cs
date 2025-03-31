using Grasshopper2;
using Grasshopper2.Data;
using Grasshopper2.Types.Assistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataType;
public sealed class QuantityTypeAssistant : TypeAssistant<Quantity>
{
    internal static readonly QuantityTypeAssistant Instance = new();
    public QuantityTypeAssistant() : base("Quantity")
    {
    }
    public override Quantity Copy(Quantity instance) => instance.Duplicate();
    public override string DescribePrimary(Pear<Quantity> pear) => pear.Item?.ToString() ?? "<empty>";
    public override bool Same(Quantity a, Quantity b)
    {
        return a.Equals(b);
    }
    public override int Sort(Quantity a, Quantity b)
    {
        if (a is not null && b is not null) return a.CompareTo(b);
        return SortRareNull(a, b);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int SortRareNull(Quantity? a, Quantity? b) 
        => a is null ? b is null ? 0 : -1 : 1;
}
