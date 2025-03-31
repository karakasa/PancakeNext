using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.Dataset;
using Pancake.GH.Params;

namespace Pancake.Component;

[ComponentCategory("qty", 1)]
public class pcToDecimalLen : PancakeComponent
{
    public override string LocalizedName => Strings.ToDecimalLength;
    public override string LocalizedDescription => Strings.ConvertAQuantityToADecimalLengthWithDesignatedUnit;
    protected override void RegisterInputs()
    {
        AddParam<GhParamQuantity>("quantity5");
        AddParam<Param_String>("unit3");
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
        string unit = null;
        DA.GetData(0, ref obj);
        DA.GetData(1, ref unit);

        if (!(obj is GhQuantity quantity))
            return;

        if (!GhDecimalLengthInfo.TryDetermineUnit(unit, out var internalUnit))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(Strings.Unit0IsNotSupported, unit));
            return;
        }

        var len = quantity.ConvertToDecimalUnit(internalUnit);

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
            return ComponentIcon.ToDecimalUnit;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("1332c8a7-f118-4319-b63d-374c5ac589a6"); }
    }
}