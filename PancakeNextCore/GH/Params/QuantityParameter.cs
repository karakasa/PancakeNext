using Grasshopper2.Parameters;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI;
using GrasshopperIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;

[IoId("1E7E4661-E60F-49A4-BEC3-4A128C1BDFB8")]
public sealed class QuantityParameter : Parameter<GhQuantity>
{
    public QuantityParameter() : base(new Nomen("Quantity", "Quantity", "Pancake", "Quantity", 0, Rank.Normal), Access.Tree)
    {
    }
    public QuantityParameter(IReader reader) : base(reader) { }
    public override ITypeAssistant<GhQuantity> TypeAssistant => QuantityTypeAssistant.Instance;
    protected override bool TryParseTypedValue(string text, out GhQuantity value)
    {
        return GhQuantity.TryParseString(text, out value!);
    }
}
