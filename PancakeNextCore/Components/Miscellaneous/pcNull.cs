using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using System;

namespace PancakeNextCore.Components.Miscellaneous;

[ComponentCategory("misc", 1)]
[IoId("41edd829-c471-42e8-ba05-6d6d35460f37")]
public sealed class pcNull : PancakeComponent<pcNull>, IPancakeLocalizable<pcNull>
{
    public pcNull() { }
    public pcNull(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.NullValues;
    public static string StaticLocalizedDescription => Strings.ProvideSpecialValuesSuchAsNullNaNInfinity;
    protected override void RegisterInputs()
    {
    }
    protected override void RegisterOutputs()
    {
        AddParam("null");
        AddParam<NumberParameter>("nan");
        AddParam<NumberParameter>("+infinity");
        AddParam<NumberParameter>("-infinity");
    }

    protected override void Process(IDataAccess access)
    {
        access.SetPear(0, null);
        access.SetItem(1, double.NaN);
        access.SetItem(2, double.PositiveInfinity);
        access.SetItem(3, double.NegativeInfinity);
    }
    protected override IIcon? IconInternal => IconHost.CreateFromPathResource("SpecialValues");
}