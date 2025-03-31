using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;

namespace Pancake.Component;

[ComponentCategory("qty", 1)]
public class pcDeconQty : PancakeComponent
{
    public override string LocalizedName => Strings.DeconstructQuantity;
    public override string LocalizedDescription => Strings.DeconstructQuantityToItsInsideAmountUnitAndUnitTypeFeetAndInchLengthWillBecomeADecimalAmountInFeetOtherwiseTheAmountIsNotConverted;
    protected override void RegisterInputs()
    {
        AddParam<GhParamQuantity>("quantity3");
    }

    protected override void RegisterOutputs()
    {
        AddParam<Param_Number>("amount2");
        AddParam<Param_String>("unit2");
        AddParam<Param_String>("unittype");
    }


    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        object obj = null;
        DA.GetData(0, ref obj);

        if (!(obj is GhQuantity quantity))
        {
            return;
        }

        DA.SetData(0, quantity.Value);
        DA.SetData(1, quantity.UnitName);
        DA.SetData(2, quantity.UnitType);
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
            return ComponentIcon.RemoveUnit;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("c5ed448a-312c-4136-b34b-839035cee90a"); }
    }
}