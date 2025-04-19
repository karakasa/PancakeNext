using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Pancake.Attributes;
using Pancake.Interfaces;
using Pancake.Utility;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcExtractFromAssoc : PancakeComponent
{
    public override string LocalizedName => Strings.DeconstructAssociativeArrayByKeys;
    public override string LocalizedDescription => Strings.RetrieveDataFromAnAssociativeArrayByKeyPaths;
    protected override void RegisterInputs()
    {
        AddParam("assoc");
        AddParam<Param_String>("path");
        AddParam("delimiter2", "/");
    }
    protected override void RegisterOutputs()
    {
        AddParam("value");
    }


    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        object assoc = null;
        DA.GetData(0, ref assoc);

        if (assoc is GH_ObjectWrapper wrapper)
            assoc = wrapper.Value;

        if (!(assoc is INodeQueryReadCapable inode))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, Strings.InputTypeNotSupported);
            return;
        }

        string txt = null;
        string delimiter = null;
        DA.GetData(1, ref txt);
        DA.GetData(2, ref delimiter);

        if (string.IsNullOrEmpty(txt))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, Strings.IncorrectPathFormat);
            return;
        }

        var path = txt.Split(new[] { delimiter }, StringSplitOptions.None);

        if (!NodeQuery.TryGetNodeValue(inode, path, out var obj))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(Strings.Path0NotFound, txt));
            return;
        }

        DA.SetData(0, obj);
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
            return ComponentIcon.DeAssocKV;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("f611c939-fe6f-449b-999d-e863608753a0"); }
    }
}