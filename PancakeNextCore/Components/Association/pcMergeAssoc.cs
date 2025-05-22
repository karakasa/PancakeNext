using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using System;
using System.Globalization;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 0)]
[IoId("6f34ada2-4487-4fd7-bca9-00dd8c500fcd")]
public sealed class pcMergeAssoc : PancakeComponent<pcMergeAssoc>, IPancakeLocalizable<pcMergeAssoc>
{
    public pcMergeAssoc() { }
    public pcMergeAssoc(IReader reader) : base(reader) { }
    public static string StaticLocalizedDescription => Strings.MergeTwoOrMoreAssociativeArrays;
    public static string StaticLocalizedName => Strings.MergeAssociativeArray;

    protected override void RegisterInputs()
    {
        AddParam<AssociationParameter>("0");
        AddParam<AssociationParameter>("1");
    }

    protected override void RegisterOutputs()
    {
        AddParam<AssociationParameter>("assoc");
    }

    protected override void Process(IDataAccess access)
    {
        var paramCount = Parameters.InputCount;
        var tuple = new GhAssoc();

        for (var i = 0; i < paramCount; i++)
        {
            if (access.GetItem(i, out GhAssoc tupleIn))
                tuple.MergeWith(tupleIn);
        }

        access.SetItem(0, tuple);
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
            Parameters.AddInput(new AssociationParameter(new("", "", ""), Access.Item), index);
        }
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
    protected override IIcon? IconInternal => IconHost.CreateFromPathResource("MergeAssoc");
}