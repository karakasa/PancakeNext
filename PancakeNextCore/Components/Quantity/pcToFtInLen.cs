using System;

using Grasshopper2.Components;
using GrasshopperIO;
using PancakeNextCore.DataType;

namespace PancakeNextCore.Components.Quantity;

[IoId("56e39add-a7bd-4eec-bed8-8dc6e528720f")]
public class pcToFtInLen : PancakeComponent
{
    public pcToFtInLen(IReader reader) : base(reader) { }
    public pcToFtInLen() : base(typeof(pcToFtInLen)) { }
    protected override void RegisterInputs()
    {
        AddParam<QuantityParameter>("quantity7");
        AddParam("precision2", 64);
    }
    protected override void RegisterOutputs()
    {
        AddParam<QuantityParameter>("quantity6");
    }
    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out DataType.Quantity quantity);
        access.GetItem(1, out int precision);

        var len = new FeetInchLength(quantity.ToNeutralUnit(), precision);
        access.SetItem(0, len);
    }
}