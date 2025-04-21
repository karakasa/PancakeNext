using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Pancake.Attributes;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PancakeNextCore.Components.Algorithm;

[ComponentCategory("misc", 0)]
public class pcMultiSort : PancakeComponent, IGH_VariableParameterComponent
{
    public override string LocalizedName => Strings.MultiSort;

    public override string LocalizedDescription => Strings.SortDataByNonNumericOrMultipleKeys;

    public override Guid ComponentGuid => new Guid("{14AF2C8E-E79F-4E95-B26E-FD6386170508}");

    private const string CfgFirstValueIndex = "FirstValueIndex";

    protected override Bitmap LightModeIcon => ComponentIcon.MultiSort;
    private int FirstValueIndex
    {
        get => GetValue(CfgFirstValueIndex, 1);
        set => SetValue(CfgFirstValueIndex, value);
    }
    public bool CanInsertParameter(GH_ParameterSide side, int index)
    {
        if (side == GH_ParameterSide.Output) return false;
        if (index == 0) return false;
        return true;
    }

    public bool CanRemoveParameter(GH_ParameterSide side, int index)
    {
        if (side == GH_ParameterSide.Output) return false;
        if (index == FirstValueIndex || index == 0) return false;
        return true;
    }

    public IGH_Param CreateParameter(GH_ParameterSide side, int index)
    {
        if (index <= FirstValueIndex)
        {
            Params.RegisterOutputParam(new Param_GenericObject()
            {
                Access = GH_ParamAccess.list
            }, index);

            ++FirstValueIndex;
            return new Param_GenericObject()
            {
                Access = GH_ParamAccess.list
            };
        }
        else
        {
            Params.RegisterOutputParam(new Param_GenericObject()
            {
                Access = GH_ParamAccess.list
            }, index);

            return new Param_GenericObject()
            {
                Access = GH_ParamAccess.list,
                Optional = true
            };
        }
    }

    public bool DestroyParameter(GH_ParameterSide side, int index)
    {
        if (index < FirstValueIndex)
            FirstValueIndex--;

        Params.UnregisterOutputParameter(Params.Output[index]);

        return true;
    }

    public void VariableParameterMaintenance()
    {
        var firstIndex = FirstValueIndex;

        for (var i = 0; i < firstIndex; i++)
        {
            Params.Input[i].NickName = $"K{i}";
            Params.Input[i].Name = $"Key {i}";
            Params.Input[i].Description = Strings.KeysToBeUsedAsSortingCriteriaOneByOne;
        }

        for (var i = firstIndex; i < Params.Input.Count; i++)
        {
            Params.Input[i].NickName = $"D{i - firstIndex}";
            Params.Input[i].Name = $"Data {i - firstIndex}";
            Params.Input[i].Description = Strings.DataToBeSortedAccordingToKeys;
        }

        for (var i = 0; i < firstIndex; i++)
        {
            Params.Output[i].NickName = $"K{i}";
            Params.Output[i].Name = $"Key {i}";
            Params.Output[i].Description = Strings.SortedKeys;
        }

        for (var i = firstIndex; i < Params.Input.Count; i++)
        {
            Params.Output[i].NickName = $"D{i - firstIndex}";
            Params.Output[i].Name = $"Data {i - firstIndex}";
            Params.Output[i].Description = Strings.DataInAccordanceToTheSortedKeys;
        }
    }

    protected override void RegisterInputs()
    {
        AddParam<Param_GenericObject>("k0in", GH_ParamAccess.list);
        AddParam<Param_GenericObject>("d0in", GH_ParamAccess.list);
        LastAddedParameter.Optional = true;
    }

    protected override void RegisterOutputs()
    {
        AddParam<Param_GenericObject>("k0out", GH_ParamAccess.list);
        AddParam<Param_GenericObject>("d0out", GH_ParamAccess.list);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var keys = new List<List<object>>();
        for (var i = 0; i < FirstValueIndex; i++)
        {
            var indivKeys = new List<object>();
            DA.GetDataList(i, indivKeys);
            keys.Add(indivKeys);
        }

        if (keys.Count == 0)
            throw new InvalidOperationException();

        var baseCnt = keys[0].Count;

        if (keys.Count > 1)
            for (var i = 1; i < keys.Count; i++)
                if (keys[i].Count != baseCnt)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.KeysMustHaveTheSameLength);
                    return;
                }

        var ordered = Enumerable.Range(0, baseCnt).OrderBy(indice => keys[0][indice], DefaultComparer);
        for (var i = 1; i < keys.Count; i++)
        {
            var localIndex = i;
            ordered = ordered.ThenBy(indice => keys[localIndex][indice], DefaultComparer);
        }

        var orderedIndices = ordered.ToArray();

        for (var i = 0; i < FirstValueIndex; i++)
        {
            DA.SetDataList(i, orderedIndices.Select(indice => keys[i][indice]));
        }

        for (var i = FirstValueIndex; i < Params.Input.Count; i++)
        {
            var listObjs = new List<object>();
            if (!DA.GetDataList(i, listObjs)) continue;

            if (listObjs.Count != baseCnt)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(Strings._0HasDifferentAmountOfDataFromKeysThereforeItSSkipped, Params.Input[i].NickName));
                continue;
            }

            DA.SetDataList(i, orderedIndices.Select(indice => listObjs[indice]));
        }
    }

    private class GooComparer : IComparer<object>
    {
        public int Compare(object x, object y)
        {
            if (x is null && y is null)
                return 0;

            if (x is null)
                return -1;

            if (y is null)
                return 1;

            if (x is GH_Integer ix && y is GH_Integer iy)
                return ix.Value.CompareTo(iy.Value);

            if (x is GH_Integer sx && y is GH_Integer sy)
                return sx.Value.CompareTo(sy.Value);

            if (TryGetNumeric(x, out var na) && TryGetNumeric(y, out var nb))
            {
                if (Math.Abs(na - nb) < RhinoMath.ZeroTolerance)
                    return 0;
                return na.CompareTo(nb);
            }

            if (x is IGH_QuickCast qx && y is IGH_QuickCast qy)
                return qx.QC_CompareTo(qy);

            return x.GetType().FullName.CompareTo(y.GetType().FullName);
        }

        private static bool TryGetNumeric(object goo, out double val)
        {
            switch (goo)
            {
                case GH_Integer integer:
                    val = integer.Value;
                    return true;
                case GH_Number number:
                    val = number.Value;
                    return true;
                case GH_Boolean boolean:
                    val = boolean.Value ? 1 : 0;
                    return true;
                case GH_Colour color:
                    val = color.Value.ToArgb();
                    return true;
                case GH_Time time:
                    val = time.Value.Ticks;
                    return true;
                default:
                    val = 0;
                    return false;
            }
        }
    }

    private static readonly GooComparer DefaultComparer = new GooComparer();
}
