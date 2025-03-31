using Grasshopper2.Parameters;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI;
using GrasshopperIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataType;

[IoId("1E7E4661-E60F-49A4-BEC3-4A128C1BDFB8")]
public sealed class QuantityParameter : Parameter<Quantity>
{
    public QuantityParameter() : base(new Nomen("Quantity", "Quantity", "Pancake", "Quantity", 0, Rank.Normal), Access.Tree)
    {
    }
    public QuantityParameter(IReader reader) : base(reader) { }
    public override ITypeAssistant<Quantity> TypeAssistant => QuantityTypeAssistant.Instance;
    protected override bool TryParseTypedValue(string text, out Quantity value)
    {
        return Quantity.TryParseString(text, out value!);
    }
}
