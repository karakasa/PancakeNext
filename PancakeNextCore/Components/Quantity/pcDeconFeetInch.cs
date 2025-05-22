using System;
using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;

namespace PancakeNextCore.Components.Quantity;

[IoId("c8e25bce-58d5-4feb-9cd1-9f77ee40c065")]
[ComponentCategory("qty", 0)]
public class pcDeconFeetInch : PancakeComponent<pcDeconFeetInch>, IPancakeLocalizable<pcDeconFeetInch>
{
    public static string StaticLocalizedName => Strings.DeconstructFeetInchLength;
    public static string StaticLocalizedDescription => Strings.DeconstructAFeetInchLengthQuantityToItsComponents;
    public pcDeconFeetInch(IReader reader) : base(reader) { }
    public pcDeconFeetInch() { }
    protected override void RegisterInputs()
    {
        AddParam<QuantityParameter>("quantity2");
    }

    protected override void RegisterOutputs()
    {
        AddParam<NumberParameter>("amountinfeet");
        AddParam<IntegerParameter>("feetinteger");
        AddParam<NumberParameter>("inch");
        AddParam<IntegerParameter>("inchinteger");
        AddParam<IntegerParameter>("inchfractionnumerator");
        AddParam<IntegerParameter>("inchfractiondenominator");
        AddParam<NumberParameter>("error");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out GhLengthFeetInch q);
        if (q == null)
        {
            access.AddError("Wrong type", Strings.WrongType);
            return;
        }

        access.SetItem(0, q.RawValue);
        access.SetItem(1, q.FeetIntegerPart * (q.IsNegative ? -1 : 1));
        access.SetItem(2, (Math.Abs(q.RawValue) - q.FeetIntegerPart) * 12);
        access.SetItem(3, q.InchIntegerPart);
        access.SetItem(4, q.InchFractionPartFirst);
        access.SetItem(5, q.InchFractionPartSecond);
        access.SetItem(6, q.Error);
    }
    protected override IIcon? IconInternal => IconHost.CreateFromPathResource("DeconstructFtInLength");
}