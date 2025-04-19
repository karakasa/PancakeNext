using System;
using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.GH.Params;

namespace PancakeNextCore.Components.Quantity;

[IoId("dc2e77c7-9bbb-4fe3-b046-088ed67d1f16")]
public class pcSetPrecision : PancakeComponent
{
    public pcSetPrecision(IReader reader) : base(reader) { }
    public pcSetPrecision() : base(typeof(pcSetPrecision)) { }
    protected override void RegisterInputs()
    {
        AddParam<QuantityParameter>("quantity4");
        AddParam<IntegerParameter>("precision2");
    }
    protected override void RegisterOutputs()
    {
        AddParam<QuantityParameter>("adjustedquantity");
        AddParam<BooleanParameter>("lostaccuracy?");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out GhQuantity obj);
        access.GetItem(1, out int precision);

        if (obj is GhLengthFeetInch len)
        {
            if (precision <= 0)
            {
                access.AddError("Wrong precision", Strings.PrecisionMustBeGreaterThan0);
                return;
            }

            var len2 = (GhLengthFeetInch)len.Duplicate();
            var oldPrecise = len.Precise;

            len2.UpdatePrecision(precision);

            access.SetItem(0, len2);
            access.SetItem(1, oldPrecise && !len2.Precise);
            return;
        }

        if (obj is GhLengthDecimal mlen)
        {
            if (precision < 0)
            {
                access.AddError("Wrong precision", Strings.PrecisionMustBeGreaterThanOrEqualTo0);
                return;
            }

            if (precision >= 10)
            {
                access.AddWarning("Wrong precision", Strings.PrecisionIsHigherThanTheInternalPrecisionYouMayNotSeeAnyChangesWithThisValue);
            }

            if (precision > 99)
            {
                access.AddError("Wrong precision", Strings.InvalidPrecision);
                return;
            }

            var mlen2 = (GhLengthDecimal)mlen.Duplicate();

            if (precision > 0)
                mlen2.KeepDecimal = true;

            if (precision == 0)
                mlen2.KeepDecimal = false;

            mlen2.Precision = precision;
            access.SetItem(0, mlen2);
            access.SetItem(1, false);
            return;
        }

        access.AddError("Unknown quantity", "The quantity is not a known type.");
    }
}