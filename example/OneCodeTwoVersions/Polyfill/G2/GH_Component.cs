using Grasshopper2.Components;
using Grasshopper2.Doc;
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

    public virtual Guid ComponentGuid => ComponentIdCacher.GetId(GetType());

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

    public new virtual System.Drawing.Icon? Icon => null;
    protected override IIcon? IconInternal
    {
        get
        {
            var icon = Icon;
            if (icon is null) return null;
            return AbstractIcon.FromBitmap([.. icon.ToEto().Frames.Select(f => f.Bitmap)]);
        }
    }
}
