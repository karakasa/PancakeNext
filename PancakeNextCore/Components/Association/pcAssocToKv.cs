using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.Interfaces;
using Pancake.Utility;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcAssocToKv : PancakeComponent
{
    public override string LocalizedName => Strings.DeconstructAssociaitveArrayAsKeyValueList;
    public override string LocalizedDescription => Strings.DeconstructAnAssociativeArrayIntoListOfKeyPathsAndValues;
    protected override void RegisterInputs()
    {
        AddParam("assoc");
        AddParam("depth", 0);
    }
    protected override void RegisterOutputs()
    {
        AddParam<Param_String>("paths", GH_ParamAccess.list);
        AddParam("value", GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        object o = null;
        var depth = 0;
        DA.GetData(0, ref o);
        DA.GetData(1, ref depth);
        if (!(o is INodeQueryReadCapable inode))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.InputTypeNotSupported);
            return;
        }

        var names = new List<string>();
        var values = new List<object>();

        foreach (var it in NodeQuery.EnumerateNode(inode, depthLimit: depth))
        {
            names.Add(it.Key);
            values.Add(it.Value);
        }

        DA.SetDataList(0, names);
        DA.SetDataList(1, values);
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
            return ComponentIcon.DeAssocKVL;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("{68D5751E-EFD3-41B5-96D0-C90466B2F58F}"); }
    }
}