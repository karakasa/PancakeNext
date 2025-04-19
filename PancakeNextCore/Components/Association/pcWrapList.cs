using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Pancake.Attributes;
using Pancake.GH.Params;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 2)]
public class pcWrapList : PancakeComponent
{
    public override string LocalizedName => Strings.WrapList;
    public override string LocalizedDescription => Strings.WrapAListToAnAtomListWhichIsAListButBeingTreatedAsOneSingleElement;
    protected override void RegisterInputs()
    {
        AddParam("items2", GH_ParamAccess.list);
        LastAddedParameter.Optional = true;
    }
    protected override void RegisterOutputs()
    {
        AddParam("atomlist2");
    }


    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var list = new List<object>();
        DA.GetDataList(0, list);

        DA.SetData(0, new GhAtomList(list));
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
            return ComponentIcon.Item2AList;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("4c770bd7-149b-42ef-85e9-81a91f7c07b5"); }
    }
}