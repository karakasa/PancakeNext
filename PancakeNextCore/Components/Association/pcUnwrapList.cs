using System;
using Grasshopper.Kernel;
using Pancake.GH.Params;
using Pancake.Attributes;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 2)]
public class pcUnwrapList : PancakeComponent
{
    public override string LocalizedName => Strings.UnwrapList;
    public override string LocalizedDescription => Strings.UnwrappedAnAtomListToItsOriginalContent;
    protected override void RegisterInputs()
    {
        AddParam("atomlist");
    }
    protected override void RegisterOutputs()
    {
        AddParam("items", GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        object input = null;
        DA.GetData(0, ref input);

        var atomList = GhAtomList.CreateFromPossibleWrapper(input);
        if (atomList == null)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.InputTypeNotSupported);
            return;
        }

        DA.SetDataList(0, atomList.GetInnerList());
        atomList = null;
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
            return ComponentIcon.AList2Item;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("0c4ec27b-fdb1-46e0-ae41-bc6f9fa1518a"); }
    }
}