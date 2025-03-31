using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;

namespace Pancake.Component;

[ComponentCategory("qty", 1)]
public class pcSetPrecision : PancakeComponent
{
    public override string LocalizedName => Strings.SetPrecision;
    public override string LocalizedDescription => Strings.SetThePrecisionOfAQuantityPrecisionMayHaveDifferentMeaningsOnDifferentQuantitiesSeeManualOrExampleForMoreInformation;
    protected override void RegisterInputs()
    {
        AddParam<GhParamQuantity>("quantity4");
        AddParam<Param_Integer>("precision2");
    }
    protected override void RegisterOutputs()
    {
        AddParam<GhParamQuantity>("adjustedquantity");
        AddParam<Param_Boolean>("lostaccuracy?");
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        object obj = null;
        DA.GetData(0, ref obj);

        if (obj is GhLengthFeetInch len)
        {
            var precision = 0;

            DA.GetData(1, ref precision);
            if (precision <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.PrecisionMustBeGreaterThan0);
                return;
            }

            var len2 = (GhLengthFeetInch)len.Duplicate();
            var oldPrecise = len.Precise;

            len2.UpdatePrecision(precision);

            DA.SetData(0, len2);
            DA.SetData(1, oldPrecise && !len2.Precise);
            return;
        }

        if(obj is GhLengthDecimal mlen)
        {
            var precision = 0;
            DA.GetData(1, ref precision);

            if (precision < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.PrecisionMustBeGreaterThanOrEqualTo0);
                return;
            }

            if (precision >= 10)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, Strings.PrecisionIsHigherThanTheInternalPrecisionYouMayNotSeeAnyChangesWithThisValue);
            }

            if (precision > 99)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.InvalidPrecision);
                return;
            }

            var mlen2 = (GhLengthDecimal)(mlen.Duplicate());

            if (precision > 0)
                mlen2.KeepDecimal = true;

            if (precision == 0)
                mlen2.KeepDecimal = false;

            mlen2.Precision = precision;
            DA.SetData(0, mlen2);
            DA.SetData(1, false);
            return;
        }

        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.InvalidQuantity);
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap LightModeIcon
    {
        get
        {
            //You can add image files to your project resources and access them like this:
            // return Resources.IconForThisComponent;
            return ComponentIcon.SetPrecision;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("dc2e77c7-9bbb-4fe3-b046-088ed67d1f16"); }
    }
}