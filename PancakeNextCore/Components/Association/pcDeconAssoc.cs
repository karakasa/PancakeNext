using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.Dataset;
using Pancake.GH.Params;
using Pancake.Interfaces;
using Pancake.UI;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcDeconAssoc : PancakeComponent, IGH_VariableParameterComponent
{
    private const string CfgAutoFill = "DeconAssoc_AutoFillOutputs";
    private const string CfgAllowAutoFill = "AllowAutoFill";
    public static bool AutoFillOutputs
    {
        get => Config.Read(CfgAutoFill, true, true);
        set => Config.Write(CfgAutoFill, value.ToString());
    }

    public bool AllowAutoFillOutputs
    {
        get => GetValue(CfgAllowAutoFill, true);
        set => SetValue(CfgAllowAutoFill, value);
    }
    public override string LocalizedDescription => Strings.DecomposeAnAssociativeArrayIntoItems;
    public override string LocalizedName => Strings.DeconstructAssociativeArray;

    /// <summary>
    /// Initializes a new instance of the pcItemFromTuple class.
    /// </summary>
    public pcDeconAssoc()
    {
    }

    public override void AddedToDocument(GH_Document document)
    {
        base.AddedToDocument(document);

        Params.ParameterNickNameChanged += NicknameChangeEvent;
    }
    public override void RemovedFromDocument(GH_Document document)
    {
        Params.ParameterNickNameChanged -= NicknameChangeEvent;

        base.RemovedFromDocument(document);
    }

    private void NicknameChangeEvent(object sender, GH_ParamServerEventArgs e)
    {
        ExpireSolution(true);
    }

    protected override void BeforeSolveInstance()
    {
        base.BeforeSolveInstance();

        if (Params.Output.Count == 0 && AllowAutoFillOutputs && AutoFillOutputs)
        {
            FillOutput(Params.Input[0].VolatileData.AllData(true).OfType<INodeQueryReadCapable>(), false, true);
        }
    }
    protected override void RegisterInputs()
    {
        AddParam("assoc");
    }

    protected override void RegisterOutputs()
    {
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        INodeQueryReadCapable node = null;
        if (!DA.GetData(0, ref node) || node == null)
            return;

        if (node is GhAssoc tuple)
        {
            for (var i = 0; i < Params.Output.Count; i++)
            {
                var nickName = Params.Output[i].NickName;
                object output = null;

                if (nickName != Params.Output[i].Name)
                {
                    if (tuple.TryGet(nickName, out output))
                        DA.SetData(i, output);
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(Strings.Key0DoesnTExist, nickName));
                        DA.SetData(i, null);
                    }
                }
                else
                {
                    if (tuple.TryGet(i, out output))
                        DA.SetData(i, output);
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(Strings.Index0DoesnTExist, i));
                        DA.SetData(i, null);
                    }
                }
            }
        }
        else
        {
            for (var i = 0; i < Params.Output.Count; i++)
            {
                var nickName = Params.Output[i].NickName;

                if (node.TryGetContent(nickName, out var content))
                {
                    DA.SetData(i, content);
                }
                else
                {
                    DA.SetData(i, null);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(Strings.Key0DoesnTExist, nickName));
                }
            }
        }
    }

    public bool CanInsertParameter(GH_ParameterSide side, int index)
    {
        return side == GH_ParameterSide.Output;
    }

    public bool CanRemoveParameter(GH_ParameterSide side, int index)
    {
        if (side != GH_ParameterSide.Output)
            return false;

        return Params.Output.Count > 1;
    }

    public IGH_Param CreateParameter(GH_ParameterSide side, int index)
    {
        AllowAutoFillOutputs = false;
        var param = new Param_GenericObject();
        param.Name = param.NickName = index.ToString();

        return param;
    }

    public bool DestroyParameter(GH_ParameterSide side, int index)
    {
        AllowAutoFillOutputs = false;
        return true;
    }

    public void VariableParameterMaintenance()
    {
        var index = 0;

        foreach (var t in Params.Output)
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

            ++index;
        }
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap LightModeIcon => ComponentIcon.Assoc2Item;

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("43dc642d-3bb6-48a2-90aa-94960105dc6f"); }
    }

    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
        Menu_AppendSeparator(menu);
        Menu_AppendItem(menu, Strings.MatchOutputWithNamedValues, MnuFillOutput);
        Menu_AppendItem(menu, Strings.FillOutputsAutomaticallyGlobal, (sender, e) => AutoFillOutputs = !AutoFillOutputs, true, AutoFillOutputs);
        Menu_AppendItem(menu, Strings.FillOutputsAutomaticallyCurrent, (sender, e) => AllowAutoFillOutputs = !AllowAutoFillOutputs, true, AllowAutoFillOutputs);
    }

    private void FillOutput(IEnumerable<INodeQueryReadCapable> assoc, bool expire = true, bool quite = false)
    {
        if (Params.Output.Any(o => o.Recipients.Count != 0))
        {
            if (quite || UiHelper.ContinueWarning(Strings.ThisOperationWouldDisconnectAllExistingRecipients))
            {
                Params.Output.ForEach(o => o.IsolateObject());
            }
            else
            {
                return;
            }
        }

        var names = assoc.SelectMany(x => x.GetAttributeNames().Concat(x.GetNodeNames()))
            .Distinct().ToArray();

        if (names.Length == 0)
        {
            if (!quite)
                UiHelper.MinorError("ItemFromAssoc.FillOutput", Strings.NoValidNamesAreFound);
            return;
        }

        Params.Output.Clear();
        Params.Output.AddRange(names.Select((n, i) =>
        {
            var param = new Param_GenericObject()
            {
                Name = i.ToString(),
                NickName = n,
                Description = $"{i} : {n}",
                Access = GH_ParamAccess.item
            };
            param.Attributes = new GH_LinkedParamAttributes(param, Attributes);
            return param;
        }
        ));

        Params.OnParametersChanged();

        if (expire)
        {
            ExpireSolution(true);
        }
    }

    private void MnuFillOutput(object sender, EventArgs e)
    {
        FillOutput(Params.Input[0].VolatileData.AllData(true)
            .OfType<GhAssoc>());
    }
}