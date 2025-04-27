using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Dataset;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;
using PancakeNextCore.UI;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 2)]
[IoId("43dc642d-3bb6-48a2-90aa-94960105dc6f")]
public sealed class pcDeconAssoc : PancakeComponent<pcDeconAssoc>, IPancakeLocalizable<pcDeconAssoc>
{
    public pcDeconAssoc() { }
    public pcDeconAssoc(IReader reader) : base(reader) { }
    private const string CfgAutoFill = "DeconAssoc_AutoFillOutputs";
    private const string CfgAllowAutoFill = "AllowAutoFill";
    public static bool GlobalAutoFillOutputs
    {
        get => Config.Read(CfgAutoFill, true, true);
        set => Config.Write(CfgAutoFill, value.ToString());
    }

    private bool _allowAutoFilleOutputs;
    public bool AllowAutoFillOutputs
    {
        get => _allowAutoFilleOutputs;
        set => SetValue(CfgAllowAutoFill, _allowAutoFilleOutputs = value);
    }
    public static string StaticLocalizedDescription => Strings.DecomposeAnAssociativeArrayIntoItems;
    public static string StaticLocalizedName => Strings.DeconstructAssociativeArray;
    protected override void ReadConfig()
    {
        if (Parameters is not null)
            Parameters.ParameterRenamed += InputParameterNameChanged;

        _allowAutoFilleOutputs = GetValue(CfgAllowAutoFill, true);
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
        AddParam<AssociationParameter>("assoc");
    }

    protected override void RegisterOutputs()
    {
    }

    private void ReadAssociation(IDataAccess access, GhAssoc tuple)
    {
        for (var i = 0; i < Parameters.OutputCount; i++)
        {
            var pOut = Parameters.Output(i);

            var nickName = pOut.UserName;
            IPear? output = null;

            if (!string.IsNullOrEmpty(nickName))
            {
                if (tuple.TryGet(nickName, out output))
                    access.SetPear(i, output);
                else
                {
                    access.AddWarning("Wrong key", string.Format(Strings.Key0DoesnTExist, nickName));
                    access.SetPear(i, null);
                }
            }
            else
            {
                if (tuple.TryGet(i, out output))
                    access.SetPear(i, output);
                else
                {
                    access.AddWarning("Wrong index", string.Format(Strings.Index0DoesnTExist, i));
                    access.SetPear(i, null);
                }
            }
        }
    }

    private void ReadGeneralObject(IDataAccess access, INodeQueryReadCapable node)
    {
        for (var i = 0; i < Parameters.OutputCount; i++)
        {
            var nickName = Parameters.Output(i).UserName;

            if (node.TryGetContent(nickName, out var content))
            {
                access.SetPear(i, content);
            }
            else
            {
                access.SetPear(i, null);
                access.AddWarning("Wrong key", string.Format(Strings.Key0DoesnTExist, nickName));
            }
        }
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out object obj);
        if (obj is not INodeQueryReadCapable node)
        {
            access.AddError("Wrong input", "Input type is not supported.");
            return;
        }

        if (node is GhAssoc tuple)
        {
            ReadAssociation(access, tuple);
        }
        else
        {
            ReadGeneralObject(access, node);
        }
    }

    public override bool CanCreateParameter(Side side, int index)
    {
        return side == Side.Output;
    }
    public override bool CanRemoveParameter(Side side, int index)
    {
        return side == Side.Output && Parameters.OutputCount > 1;
    }
    public override void DoCreateParameter(Side side, int index)
    {
        AllowAutoFillOutputs = false;

        var param = new GenericParameter("", "", "", Access.Item).With(Requirement.MayBeNull);
        Parameters.AddInput(param);
    }
    public override void DoRemoveParameter(Side side, int index)
    {
        AllowAutoFillOutputs = false;
        base.DoRemoveParameter(side, index);
    }
    public override void VariableParameterMaintenance()
    {
        var index = 0;

        foreach (var t in Parameters.Outputs)
        {
            var indStr = index.ToString(CultureInfo.InvariantCulture);
            if (t.Nomen.Name == "")
            {
                t.ModifyNameAndInfo(indStr, $"Item {indStr}");
            }
            else if (!string.IsNullOrEmpty(t.UserName))
            {
                t.ModifyNameAndInfo(t.Nomen.Name, $"Item {indStr} : {t.UserName}");
            }

            ++index;
        }
    }

    protected override void BeforeProcess(Solution solution)
    {
        base.BeforeProcess(solution);

        if (Parameters.OutputCount == 0 && AllowAutoFillOutputs && GlobalAutoFillOutputs && Parameters.Input(0).Inputs.Count > 0)
        {
            FillOutputFromInput(true);
        }
    }

    private void FillOutputFromInput(bool quiet)
    {
        FillOutput(TryGetAssociations(Parameters.Input(0)), quiet: quiet);
    }

    private static IEnumerable<INodeQueryReadCapable> TryGetAssociations(IParameter param)
    {
        var tree = param.State?.Data?.Tree();
        if (tree is null) yield break;
        foreach (var it in tree.AllPears)
        {
            if (it.Type.IsValueType) continue;
            if (it.Item is INodeQueryReadCapable g) yield return g;
        }
    }

    protected override InputOption[][] SimpleOptions =>
        [[
            new ToggleOption("Allow auto fill for this component", Strings.FillOutputsAutomaticallyCurrent,
                AllowAutoFillOutputs, x => AllowAutoFillOutputs = x, "Fill this", "Skip", "Auto fill")
                { OnColor = OpenColor.Blue7 },

            new ToggleOption("Allow auto fill for this globally", Strings.FillOutputsAutomaticallyGlobal,
                GlobalAutoFillOutputs, x => GlobalAutoFillOutputs = x, "Fill all", "Skip", "Auto fill")
                { OnColor = OpenColor.Blue7 },
        ],
        [
            new ButtonOption("Fill outputs now", "Fill outputs immediately, from input association to this component.", MnuClickFill)
        ]];

    private void MnuClickFill()
    {
        FillOutputFromInput(false);
    }

    private bool TestOutputForFill(bool quite = false)
    {
        if (Parameters.Outputs.Any(o => o.Outputs.Count != 0))
        {
            if (quite || UiHelper.ContinueWarning(Strings.ThisOperationWouldDisconnectAllExistingRecipients))
            {
                Parameters.Outputs.ForEach(o => Connections.DisconnectAllOutputs(o));
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private void FillOutput(IEnumerable<INodeQueryReadCapable> assoc, bool expire = true, bool quiet = false)
    {
        if (assoc is null) return;

        var names = assoc.SelectMany(x => x.GetAttributeNames().Concat(x.GetNodeNames()))
            .Distinct().ToArray();

        if (names.Length == 0)
        {
            if (!quiet)
                UiHelper.MinorError("ItemFromAssoc.FillOutput", Strings.NoValidNamesAreFound);
            return;
        }

        if (!TestOutputForFill(quiet)) return;

        var cnt = Parameters.OutputCount;
        for (var i = cnt - 1; i >= 0; i++)
        {
            Parameters.RemoveOutput(i);
        }

        foreach (var name in names)
        {
            var param = new GenericParameter("", "", "", Access.Item);
            Parameters.AddOutput(param);
        }

        if (expire)
        {
            ExpireSolution(true);
        }
    }
}