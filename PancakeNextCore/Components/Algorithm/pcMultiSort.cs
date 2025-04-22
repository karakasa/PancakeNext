using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PancakeNextCore.Components.Algorithm;

[ComponentCategory("misc", 0)]
[IoId("{14AF2C8E-E79F-4E95-B26E-FD6386170508}")]
public sealed class pcMultiSort : PancakeComponent<pcMultiSort>, IPancakeLocalizable<pcMultiSort>
{
    public pcMultiSort() { }
    public pcMultiSort(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.MultiSort;

    public static string StaticLocalizedDescription => Strings.SortDataByNonNumericOrMultipleKeys;

    private const string CfgFirstValueIndex = "FirstValueIndex";
    private int _firstValueIndex;
    private int FirstValueIndex
    {
        get => _firstValueIndex;
        set => SetValue(CfgFirstValueIndex, _firstValueIndex = value);
    }
    protected override void ReadConfig()
    {
        _firstValueIndex = GetValue(CfgFirstValueIndex, 1);
    }

    public override bool CanCreateParameter(Side side, int index)
    {
        return side is Side.Input && index != 0;
    }

    public override bool CanRemoveParameter(Side side, int index)
    {
        return side is Side.Input && index != FirstValueIndex && index != 0;
    }

    public override void DoCreateParameter(Side side, int index)
    {
        if (index <= FirstValueIndex)
        {
            Parameters.AddOutput(new GenericParameter("", "", "", Access.Twig), index);
            Parameters.AddInput(new GenericParameter("", "", "", Access.Twig), index);
            ++FirstValueIndex;
        }
        else
        {
            Parameters.AddOutput(new GenericParameter("", "", "", Access.Twig), index);
            Parameters.AddInput(new GenericParameter("", "", "", Access.Twig).With(Requirement.MayBeMissing), index);
        }
    }

    public override void DoRemoveParameter(Side side, int index)
    {
        if (index < FirstValueIndex) --FirstValueIndex;

        Parameters.RemoveOutput(index);
        base.DoRemoveParameter(side, index);
    }

    public override void VariableParameterMaintenance()
    {
        var firstIndex = FirstValueIndex;

        for (var i = 0; i < firstIndex; i++)
        {
            var p = Parameters.Input(i);

            p.UserName = $"K{i}";
            p.ModifyNameAndInfo($"Key {i}", Strings.KeysToBeUsedAsSortingCriteriaOneByOne);
        }

        for (var i = firstIndex; i < Parameters.InputCount; i++)
        {
            var p = Parameters.Input(i);

            p.UserName = $"D{i - firstIndex}";
            p.ModifyNameAndInfo($"Data {i - firstIndex}", Strings.DataToBeSortedAccordingToKeys);
        }

        for (var i = 0; i < firstIndex; i++)
        {
            var p = Parameters.Output(i);

            p.UserName = $"K{i}";
            p.ModifyNameAndInfo($"Key {i}", Strings.SortedKeys);
        }

        for (var i = firstIndex; i < Parameters.InputCount; i++)
        {
            var p = Parameters.Output(i);

            p.UserName = $"D{i - firstIndex}";
            p.ModifyNameAndInfo($"Data {i - firstIndex}", Strings.DataInAccordanceToTheSortedKeys);
        }
    }

    protected override void RegisterInputs()
    {
        AddParam<GenericParameter>("k0in", Access.Twig);
        AddParam<GenericParameter>("d0in", Access.Twig, Requirement.MayBeMissing);
    }

    protected override void RegisterOutputs()
    {
        AddParam<GenericParameter>("k0out", Access.Twig);
        AddParam<GenericParameter>("d0out", Access.Twig);
    }

    protected override void Process(IDataAccess access)
    {
        var keys = new List<ITwig>();
        for (var i = 0; i < FirstValueIndex; i++)
        {
            access.GetITwig(i, out var indivKeys);
            keys.Add(indivKeys);
        }

        if (keys.Count == 0)
            throw new InvalidOperationException();

        var baseCnt = keys[0].ItemCount;

        if (keys.Count > 1)
            for (var i = 1; i < keys.Count; i++)
                if (keys[i].ItemCount != baseCnt)
                {
                    access.AddError("List mismatch", Strings.KeysMustHaveTheSameLength);
                    return;
                }

        var ordered = Enumerable.Range(0, baseCnt)
            .OrderBy(indice => keys[0][indice], PearComparerGeneric.Instance);
        for (var i = 1; i < keys.Count; i++)
        {
            var localIndex = i;
            ordered = ordered.ThenBy(indice => keys[localIndex][indice], PearComparerGeneric.Instance);
        }

        var orderedIndices = ordered.ToArray();

        for (var i = 0; i < FirstValueIndex; i++)
        {
            access.SetTwig(i, Garden.ITwigFromPears(orderedIndices.Select(indice => keys[i][indice])));
        }

        for (var i = FirstValueIndex; i < Parameters.InputCount; i++)
        {
            if (!access.GetITwig(i, out var listObjs)) continue;

            if (listObjs.ItemCount != baseCnt)
            {
                access.AddWarning("List mismatch", string.Format(Strings._0HasDifferentAmountOfDataFromKeysThereforeItSSkipped,
                    Parameters.Input(i).DisplayName));
                continue;
            }

            access.SetTwig(i, Garden.ITwigFromPears(orderedIndices.Select(indice => listObjs[indice])));
        }
    }
}
