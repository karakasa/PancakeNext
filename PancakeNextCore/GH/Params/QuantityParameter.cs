using Grasshopper2.Interop;
using Grasshopper2.Parameters;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;

[IoId("1E7E4661-E60F-49A4-BEC3-4A128C1BDFB8")]
[ComponentCategory("qty", 0)]
public sealed class QuantityParameter : PancakeParameter<GhQuantity, QuantityParameter>, IPancakeLocalizable<QuantityParameter>
{
    public QuantityParameter() { }
    public QuantityParameter(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.Quantity;
    public static string StaticLocalizedDescription => Strings.RepresentAnAmountAndItsAssociatedLengthUnit;
    public override ITypeAssistant<GhQuantity> TypeAssistant => QuantityTypeAssistant.Instance;
    protected override bool TryParseTypedValue(string text, out GhQuantity value)
    {
        return GhQuantity.TryParseString(text, out value!);
    }
   
}
