using System;

using Grasshopper2.Components;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;

namespace PancakeNextCore.Components.Quantity;

[IoId("56e39add-a7bd-4eec-bed8-8dc6e528720f")]
[ComponentCategory("qty", 0)]
public sealed class pcToFtInLen : PancakeComponent<pcToFtInLen>, IPancakeLocalizable<pcToFtInLen>
{
    public static string StaticLocalizedName => Strings.ToFeetInchLength;
    public static string StaticLocalizedDescription => Strings.ConvertAQuantityToAFeetInchLengthWithDesignatedUnit;
    public pcToFtInLen(IReader reader) : base(reader) { }
    public pcToFtInLen() { }
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

        access.GetItem(0, out GhQuantity quantity);
        access.GetItem(1, out int precision);

        var len = new GhLengthFeetInch(quantity.ToNeutralUnit(), precision);
        access.SetItem(0, len);
    }
    protected override IIcon? IconInternal => IconHost.CreateFromPathResource("ToFtInLength");
}