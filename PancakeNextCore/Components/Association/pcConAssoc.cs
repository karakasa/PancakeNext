using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcConAssoc : PancakeComponent, IGH_VariableParameterComponent
{
    public override string LocalizedName => Strings.ConstructAssociativeArray;
    public override string LocalizedDescription => Strings.CreateAnAssociativeArrayFromAListOfItemsAnAssociativeArrayWillBeTreatedAsOneSingleElementDuringGrasshopperTreeManipulationThereforeStructureManipulationWouldBeEasier;

    /// <summary>
    /// Initializes a new instance of the pcTupleFromItem class.
    /// </summary>
    public pcConAssoc()
    {
        // Params.ParameterNickNameChanged += NicknameChangeEvent;
        Params.ParameterChanged += ParamObjectChanged;
    }

    private void ParamObjectChanged(object sender, GH_ParamServerEventArgs e)
    {
        if (e.OriginalArguments.Type == GH_ObjectEventType.NickNameAccepted)
            ExpireSolution(true);
    }

    private void NicknameChangeEvent(object sender, GH_ParamServerEventArgs e)
    {
        ExpireSolution(true);
    }

    public override void RemovedFromDocument(GH_Document document)
    {
        Params.ParameterNickNameChanged -= NicknameChangeEvent;
    }

    protected override void RegisterInputs()
    {
        AddParam("0");
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
        var paramCount = Params.Input.Count;
        var tuple = new GhAssoc(paramCount);

        for (var i = 0; i < paramCount; i++)
        {
            object data = null;
            DA.GetData(i, ref data);
            if (Params.Input[i].NickName != Params.Input[i].Name)
            {
                tuple.Add(Params.Input[i].NickName, data);
            }
            else
            {
                tuple.Add(data);
            }
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
        return true;
    }

    public IGH_Param CreateParameter(GH_ParameterSide side, int index)
    {
        var param = new Param_GenericObject();
        param.Optional = true;
        param.Name = param.NickName = index.ToString();

        return param;
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
            if (t.Name == t.NickName)
            {
                t.Name = t.NickName = index.ToString();
                t.Description = $"{index}";
            }
            else
            {
                t.Name = index.ToString();
                t.Description = $"{index} : {t.NickName}";
            }

            t.Optional = true;

            ++index;
        }
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap LightModeIcon => ComponentIcon.Item2Assoc;

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("6f34ada2-4487-4fd7-bca9-00dd8c5eefcd"); }
    }
}