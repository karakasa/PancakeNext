using System;

using Grasshopper.Kernel;
using Pancake.Attributes;
using Pancake.GH.Params;

namespace Pancake.Component;

[ComponentCategory("qty", 1)]
public class pcToFtInLen : PancakeComponent
{
    public override string LocalizedName => Strings.ToFeetInchLength;
    public override string LocalizedDescription => Strings.ConvertAQuantityToAFeetInchLengthWithDesignatedUnit;
    protected override void RegisterInputs()
    {
        AddParam<GhParamQuantity>("quantity7");
        AddParam("precision2", 64);
    }
    protected override void RegisterOutputs()
    {
        AddParam<GhParamQuantity>("quantity6");
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        object obj = null;
        var precision = 4;
        DA.GetData(0, ref obj);
        DA.GetData(1, ref precision);

        if(!(obj is GhQuantity quantity))
            return;

        var len = new GhLengthFeetInch(quantity.ToNeutralUnit(), precision);
        DA.SetData(0, len);
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
            return ComponentIcon.ToFtIn;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("56e39add-a7bd-4eec-bed8-8dc6e528720f"); }
    }
}