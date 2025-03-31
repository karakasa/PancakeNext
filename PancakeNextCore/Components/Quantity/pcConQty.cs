using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Dataset;
using PancakeNextCore.DataType;
using PancakeNextCore.Utility;
using System;

namespace PancakeNextCore.Components.Quantity;

[IoId("27155ea3-5697-4d29-8e8c-c0487821ce59")]
public sealed class pcConQty : PancakeComponent
{
    public pcConQty(IReader reader) : base(reader) { }
    public pcConQty() : base(typeof(pcConQty)) { }
    protected override void RegisterInputs()
    {
        AddParam<NumberParameter>("amount");
        AddParam<TextParameter>("unit", requirement: Requirement.MayBeMissing);
        AddParam("precision", 4);
    }
    protected override void RegisterOutputs()
    {
        AddParam<QuantityParameter>("quantity");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out double amount);
        access.GetItem(2, out int precision);
        var unitAvailable = access.GetItem(1, out string unit);

        if (string.IsNullOrEmpty(unit))
            unitAvailable = false;

        DecimalLength len;

        if (unitAvailable)
        {
            if (!DecimalLengthInfo.TryDetermineUnit(unit, out var internalUnit))
            {
                access.AddError("Unsupported unit", $"{unit} is not supported.");
                return;
            }

            len = new DecimalLength(amount, internalUnit, precision);
        }
        else
        {
            var doc = RhinoDocServer.ActiveDoc;
            if (doc == null)
            {
                access.AddError("Cannot infer unit", "Current model unit is unavailable.");
                return;
            }
            var docUnit = doc.ModelUnitSystem;

            if (!DecimalLengthInfo.TryDetermineUnit(docUnit, out var rhinoUnit))
            {
                rhinoUnit = DecimalLengthInfo.NeutralUnit;
            }

            len = new DecimalLength(rhinoUnit, precision);
            len.FromDocumentUnit(amount);
        }

        access.SetItem(0, len);
    }
}