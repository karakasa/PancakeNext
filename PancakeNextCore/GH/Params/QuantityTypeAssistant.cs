using Grasshopper2;
using Grasshopper2.Data;
using Grasshopper2.Types.Assistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
public sealed class QuantityTypeAssistant : TypeAssistant<GhQuantity>
{
    internal static readonly QuantityTypeAssistant Instance = new();
    public QuantityTypeAssistant() : base("Quantity")
    {
    }
    public override GhQuantity Copy(GhQuantity instance) => instance.Duplicate();
    public override string DescribePrimary(Pear<GhQuantity> pear) => pear.Item?.ToString() ?? "<empty>";
    public override bool Same(GhQuantity a, GhQuantity b)
    {
        return a.Equals(b);
    }
    public override int Sort(GhQuantity a, GhQuantity b)
    {
        if (a is not null && b is not null) return a.CompareTo(b);
        return SortRareNull(a, b);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int SortRareNull(GhQuantity? a, GhQuantity? b)
        => a is null ? b is null ? 0 : -1 : 1;
}
