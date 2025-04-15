using System;
using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Dataset;
using PancakeNextCore.DataType;

namespace PancakeNextCore.Components.Quantity;

[IoId("1332c8a7-f118-4319-b63d-374c5ac589a6")]
public class pcToDecimalLen : PancakeComponent
{
    public pcToDecimalLen(IReader reader) : base(reader) { }
    public pcToDecimalLen() : base(typeof(pcToDecimalLen)) { }
    protected override void RegisterInputs()
    {
        AddParam<QuantityParameter>("quantity5");
        AddParam<TextParameter>("unit3");
    }
    protected override void RegisterOutputs()
    {
        AddParam<QuantityParameter>("quantity6");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out DataType.GhQuantity quantity);
        access.GetItem(1, out string unit);

        if (!GhDecimalLengthInfo.TryDetermineUnit(unit, out var internalUnit))
        {
            access.AddError("Wrong unit", string.Format(Strings.Unit0IsNotSupported, unit));
            return;
        }

        var len = quantity.ConvertToDecimalUnit(internalUnit);
        access.SetItem(0, len);
    }
}