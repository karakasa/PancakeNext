using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Pancake.Attributes;

namespace PancakeNextCore.Components.Algorithm;

[ComponentCategory("data", 0)]
public class pcShuffle : PancakeComponent
{
    public override string LocalizedName => Strings.Shuffle;
    public override string LocalizedDescription => Strings.UseFisherYatesAlgorithmToShuffleAList;
    protected override void RegisterInputs()
    {
        AddParam("list", GH_ParamAccess.list);
        AddParam("seed", 0);
    }
    protected override void RegisterOutputs()
    {
        AddParam("list2", GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var seed = 0;
        var list = new List<object>();

        DA.GetDataList(0, list);
        DA.GetData(1, ref seed);

        if (list.Count == 0)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, Strings.TheListCannotBeEmpty);
            return;
        }

        if (list.Count == 1)
        {
            DA.SetDataList(0, list);
            return;
        }

        var randomGenerator = new Random(seed);
        var n = list.Count;
        while (n > 1)
        {
            --n;
            var k = randomGenerator.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        DA.SetDataList(0, list);
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
            return ComponentIcon.Shuffle;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("b01bc8ba-c031-4ccc-a28a-cd3f81376e00"); }
    }
}