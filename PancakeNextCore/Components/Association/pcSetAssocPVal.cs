using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcSetAssocPVal : PancakeComponent
{
    public override string LocalizedName => Strings.SetPrincipleValue;
    public override string LocalizedDescription => Strings.SetPValDesc;
    protected override void RegisterInputs()
    {
        AddParam("assoc");
        AddParam<Param_Integer>("index");
        LastAddedParameter.Optional = true;
        AddParam<Param_String>("name2");
        LastAddedParameter.Optional = true;
    }
    protected override void RegisterOutputs()
    {
        AddParam("assoc");
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GhAssoc assoc = null;
        var index = 0;
        var name = "";

        if (!DA.GetData(0, ref assoc))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.InputIsNotAnAssoc);
            return;
        }

        var indexAvailable = DA.GetData(1, ref index);
        var nameAvailable = DA.GetData(2, ref name);

        if (!(indexAvailable ^ nameAvailable))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                Strings.YouShouldProvideEitherIndexOrNameInsteadOfNoneOrBoth);
            return;
        }

        var assoc2 = (GhAssoc)assoc.Duplicate();

        if (indexAvailable)
        {
            if (!assoc2.SetPrincipleValue(index))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(Strings.Index0IsOutOfRange, index));
                return;
            }
        }
        else
        {
            if (!assoc2.SetPrincipleValue(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(Strings._0IsNotAValidName, name));
                return;
            }
        }

        DA.SetData(0, assoc2);
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
            return ComponentIcon.SetPrinciple;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("cdfa0604-d6a1-4179-8886-987788ee9b64"); }
    }

}