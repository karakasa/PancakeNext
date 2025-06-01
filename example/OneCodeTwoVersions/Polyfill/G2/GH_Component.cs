using Grasshopper2.Components;
using Grasshopper2.Data.Meta;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract partial class GH_Component : Component, IGH_ActiveObject
{
    protected GH_Component(Nomen nomen) : base(nomen) { }

    protected GH_Component(IReader reader) : base(reader) { }
    public string Name
    {
        get => Nomen.Name;
        set => ModifyNameAndInfo(value, null);
    }
    public string NickName
    {
        get => UserName;
        set => UserName = value;
    }
    public string Description
    {
        get => Nomen.Info;
        set => ModifyNameAndInfo(null, value);
    }
    public string Category => Nomen.Chapter;
    public string SubCategory => Nomen.Section;
    public Guid InstanceGuid => InstanceId;

    public bool Locked
    {
        get => Activity == ObjectActivity.Disabled;
        set => Activity = value ? ObjectActivity.Disabled : ObjectActivity.Enabled;
    }

    public virtual Guid ComponentGuid => ComponentIdCacher.Instance[GetType()];

    protected abstract void RegisterInputParams(GH_InputParamManager pManager);

    protected abstract void RegisterOutputParams(GH_OutputParamManager pManager);
    protected abstract void SolveInstance(IGH_DataAccess DA);

    protected override void AddInputs(InputAdder inputs)
    {
        var manager = new GH_InputParamManager(this, inputs);
        RegisterInputParams(manager);
    }

    protected override void AddOutputs(OutputAdder outputs)
    {
        var manager = new GH_OutputParamManager(this, outputs);
        RegisterOutputParams(manager);
    }

    internal IDataAccess? CurrentAccess { get; private set; }

    protected override void Process(IDataAccess access)
    {
        try
        {
            CurrentAccess = access;
            SolveInstance(new DataAccessWrapper(this));
        }
        finally
        {
            CurrentAccess = null;
        }
    }
    public virtual void AddRuntimeMessage(GH_RuntimeMessageLevel level, string text)
    {
        if (CurrentAccess is null) return;

        switch (level)
        {
            case GH_RuntimeMessageLevel.Remark:
                CurrentAccess.AddRemark("Remark", text);
                break;
            case GH_RuntimeMessageLevel.Warning:
                CurrentAccess.AddWarning("Warning", text);
                break;
            case GH_RuntimeMessageLevel.Error:
                CurrentAccess.AddError("Error", text);
                break;
        }
    }

    protected override void BeforeProcess(Solution solution)
    {
        base.BeforeProcess(solution);

        BeforeSolveInstance();
    }

    protected override void PostProcess(Solution solution)
    {
        base.PostProcess(solution);

        AfterSolveInstance();
    }
    protected virtual void AfterSolveInstance() { }
    protected virtual void BeforeSolveInstance() { }
    public string Message
    {
        get => Label;
        set => Label = value;
    }
    public new GH_SolutionPhase Phase => base.Phase.To1();

    public new virtual System.Drawing.Bitmap? Icon => null;
    protected override IIcon? IconInternal
    {
        get
        {
            var icon = Icon;
            if (icon is null) return null;
            return AbstractIcon.FromBitmap(icon.ToEto());
        }
    }

    private GH_ComponentParamServer? _params;
    public GH_ComponentParamServer Params => _params ??= new(this);
    protected bool GetValue(string valueName, bool @default) => CustomValues.Get(valueName, @default);

    protected int GetValue(string valueName, int @default) => CustomValues.Get(valueName, @default);

    protected double GetValue(string valueName, double @default) => CustomValues.Get(valueName, @default);

    protected string GetValue(string valueName, string @default) => CustomValues.Get(valueName, @default);

    protected System.Drawing.Color GetValue(string valueName, System.Drawing.Color @default) => CustomValues.Get(valueName, ColourExtensionMethods.ToEto(@default)).ToGdi();

    protected void SetValue(string valueName, bool value) => CustomValues.Set(valueName, value);

    protected void SetValue(string valueName, int value) => CustomValues.Set(valueName, value);

    protected void SetValue(string valueName, double value) => CustomValues.Set(valueName, value);
    protected void SetValue(string valueName, string value) => CustomValues.Set(valueName, value);

    protected void SetValue(string valueName, System.Drawing.Color value) => CustomValues.Set(valueName, ColourExtensionMethods.ToEto(value));
    protected void ExpireSolution(bool recompute)
    {
        if (recompute)
        {
            Document?.Solution.DelayedExpire(this);
        }
        else
        {
            State?.Expire();
        }
    }
}
