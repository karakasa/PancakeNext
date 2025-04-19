using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcMergeAssoc : PancakeComponent, IGH_VariableParameterComponent
{
    public override string LocalizedDescription => Strings.MergeTwoOrMoreAssociativeArrays;
    public override string LocalizedName => Strings.MergeAssociativeArray;

    protected override void RegisterInputs()
    {
        AddParam("0");
        AddParam("1");
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
        var paramCount = Params.Input.Count;
        var tuple = new GhAssoc();

        for (var i = 0; i < paramCount; i++)
        {
            GhAssoc tupleIn = null;
            DA.GetData(i, ref tupleIn);
            tuple.MergeWith(tupleIn);
        }

        DA.SetData(0, tuple);
    }

    public bool CanInsertParameter(GH_ParameterSide side, int index)
    {
        if (side == GH_ParameterSide.Output)
            return false;
        return true;
    }

    public bool CanRemoveParameter(GH_ParameterSide side, int index)
    {
        if (side == GH_ParameterSide.Output)
            return false;
        return Params.Input.Count > 2;
    }

    public IGH_Param CreateParameter(GH_ParameterSide side, int index)
    {
        return new Param_GenericObject();
    }

    public bool DestroyParameter(GH_ParameterSide side, int index)
    {
        return true;
    }

    public void VariableParameterMaintenance()
    {
        var index = 0;

        foreach (var t in Params.Input)
        {
            t.Name = t.NickName = index.ToString();
            t.Description = $"{index}";

            ++index;
        }
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap LightModeIcon => ComponentIcon.MergeAssoc;

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("6f34ada2-4487-4fd7-bca9-00dd8c500fcd"); }
    }

}