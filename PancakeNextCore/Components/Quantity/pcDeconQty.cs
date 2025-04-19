using System;
using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.GH.Params;

namespace PancakeNextCore.Components.Quantity;

[IoId("c5ed448a-312c-4136-b34b-839035cee90a")]
public class pcDeconQty : PancakeComponent
{
    public pcDeconQty(IReader reader) : base(reader) { }
    public pcDeconQty() : base(typeof(pcDeconQty)) { }
    protected override void RegisterInputs()
    {
        AddParam<QuantityParameter>("quantity3");
    }

    protected override void RegisterOutputs()
    {
        AddParam<NumberParameter>("amount2");
        AddParam<TextParameter>("unit2");
        AddParam<TextParameter>("unittype");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out GH.Params.GhQuantity quantity);

        access.SetItem(0, quantity.GetRawValue());
        access.SetItem(1, quantity.UnitName);
        access.SetItem(2, quantity.UnitType);
    }
}