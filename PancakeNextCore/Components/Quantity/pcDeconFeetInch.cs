using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;

namespace Pancake.Component;

[ComponentCategory("qty", 1)]
public class pcDeconFeetInch : PancakeComponent
{
    public override string LocalizedName => Strings.DeconstructFeetInchLength;
    public override string LocalizedDescription => Strings.DeconstructAFeetInchLengthQuantityToItsComponents;
    protected override void RegisterInputs()
    {
        AddParam<GhParamQuantity>("quantity2");
    }

    protected override void RegisterOutputs()
    {
        AddParam<Param_Number>("amountinfeet");
        AddParam<Param_Integer>("feetinteger");
        AddParam<Param_Number>("inch");
        AddParam<Param_Integer>("inchinteger");
        AddParam<Param_Integer>("inchfractionnumerator");
        AddParam<Param_Integer>("inchfractiondenominator");
        AddParam<Param_Number>("error");
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GhLengthFeetInch q = null;
        DA.GetData(0, ref q);
        if (q == null)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.WrongType);
            return;
        }

        DA.SetData(0, q.Value);
        DA.SetData(1, q.FeetIntegerPart * (q.Negative ? (-1) : 1));
        DA.SetData(2, (Math.Abs(q.Value) - q.FeetIntegerPart) * 12);
        DA.SetData(3, q.InchIntegerPart);
        DA.SetData(4, q.InchFractionPartFirst);
        DA.SetData(5, q.InchFractionPartSecond);
        DA.SetData(6, q.Error);
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
            return ComponentIcon.DeconFtInch;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("c8e25bce-58d5-4feb-9cd1-9f77ee40c065"); }
    }
}