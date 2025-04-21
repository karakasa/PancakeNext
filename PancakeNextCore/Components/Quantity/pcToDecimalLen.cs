using System;
using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Dataset;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;

namespace PancakeNextCore.Components.Quantity;

[IoId("1332c8a7-f118-4319-b63d-374c5ac589a6")]
[ComponentCategory("qty", 0)]
public sealed class pcToDecimalLen : PancakeComponent<pcToDecimalLen>, IPancakeLocalizable<pcToDecimalLen>
{
    public static string StaticLocalizedName => Strings.ToDecimalLength;

    public static string StaticLocalizedDescription => Strings.ConvertAQuantityToADecimalLengthWithDesignatedUnit;

    public pcToDecimalLen(IReader reader) : base(reader) { }
    public pcToDecimalLen() { }
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

        access.GetItem(0, out GH.Params.GhQuantity quantity);
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