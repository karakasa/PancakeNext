using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;
using System;
using System.Globalization;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
[IoId("6f34ada2-4487-4fd7-bca9-00dd8c5eefcd")]
public sealed class pcConAssoc : PancakeComponent<pcConAssoc>, IPancakeLocalizable<pcConAssoc>
{
    public static string StaticLocalizedName => Strings.ConstructAssociativeArray;
    public static string StaticLocalizedDescription => Strings.CreateAnAssociativeArrayFromAListOfItemsAnAssociativeArrayWillBeTreatedAsOneSingleElementDuringGrasshopperTreeManipulationThereforeStructureManipulationWouldBeEasier;

    public pcConAssoc() { }
    public pcConAssoc(IReader reader) : base(reader) { }

    protected override void ReadConfig()
    {
        if (Parameters is not null)
            Parameters.ParameterRenamed += InputParameterNameChanged;
    }

    private void InputParameterNameChanged(object? sender, ParameterEventArgs e)
    {
        if (e.Side == Side.Input)
        {
            ExpireSolution(true);
        }
    }

    protected override void RegisterInputs()
    {
        AddParam("0", requirement: Requirement.MayBeNull);
    }

    protected override void RegisterOutputs()
    {
        AddParam<AssociationParameter>("assoc");
    }

    public override bool CanCreateParameter(Side side, int index)
    {
        return side == Side.Input;
    }

    public override bool CanRemoveParameter(Side side, int index)
    {
        return side == Side.Input;
    }

    public override void DoCreateParameter(Side side, int index)
    {
        if (side == Side.Input)
        {
            Parameters.AddInput(new GenericParameter("", "", "", Access.Item), index);
        }
    }

    protected override void Process(IDataAccess access)
    {
        var paramCount = Parameters.InputCount;
        var tuple = new GhAssoc(paramCount);

        for (var i = 0; i < paramCount; i++)
        {
            var inp = Parameters.Input(i);
            access.GetIPear(i, out var data);
            if (string.IsNullOrEmpty(inp.UserName))
            {
                tuple.Add(data);
            }
            else
            {
                tuple.Add(inp.UserName, data);
            }
        }

        access.SetItem(0, tuple);
    }

    public override void VariableParameterMaintenance()
    {
        var index = 0;

        foreach (var t in Parameters.Inputs)
        {
            t.Requirement = Requirement.MayBeNull;

            if (t.Nomen.Name == "") // newly created
            {
                var indexStr = index.ToString(CultureInfo.InvariantCulture);
                t.ModifyNameAndInfo(indexStr, $"Item {indexStr}");
                t.FallbackName = indexStr;
            }

            ++index;
        }
    }
}