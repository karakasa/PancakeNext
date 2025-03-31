using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Rhino.UI;

namespace SimpleCompatibleLayer;

public abstract class CompatibleComponent : Component
{
    protected sealed class GH_InputParamManager
    {

    }
    protected sealed class GH_OutputParamManager
    {

    }
    protected CompatibleComponent(string name, string nickname, string description, string category, string subCategory)
        : base(new Nomen(name, description, category, subCategory))
    {
        _dataAccessWrapper = new(this);
    }

    protected CompatibleComponent(IReader reader) : base(reader)
    {
        _dataAccessWrapper = new(this);
    }

    private sealed class DataAccessWrapper(CompatibleComponent comp) : IGH_DataAccess
    {
        private readonly CompatibleComponent _comp = comp;
    }

    private readonly DataAccessWrapper _dataAccessWrapper;
    protected abstract void RegisterInputParams(GH_InputParamManager pManager);
    protected abstract void RegisterOutputParams(GH_OutputParamManager pManager);
    protected abstract void SolveInstance(IGH_DataAccess DA);
    protected new virtual System.Drawing.Bitmap? Icon { get; }
    protected override IIcon? IconInternal
    {
        get
        {
            var icon = Icon;
            if (icon is null) return null;

            return AbstractIcon.FromBitmap(icon.ToEto());
        }
    }
    public abstract Guid ComponentGuid { get; }
    protected override void AddInputs(InputAdder inputs)
    {
        throw new NotImplementedException();
    }
    protected override void AddOutputs(OutputAdder outputs)
    {
        throw new NotImplementedException();
    }
    private IDataAccess? _currentAccess = null;
    protected override void Process(IDataAccess access)
    {
        try
        {
            _currentAccess = access;
            SolveInstance(_dataAccessWrapper);
        }
        finally
        {
            _currentAccess = null;
        }
    }
    public virtual bool Write(GH_IWriter writer)
    {
        throw new NotSupportedException();
    }
    public virtual bool Read(GH_IReader writer)
    {
        throw new NotSupportedException();
    }

    public virtual void AddRuntimeMessage(GH_RuntimeMessageLevel level, string text)
    {
        if (_currentAccess is null)
        {
            throw new InvalidOperationException("Runtime messages are not permitted outside SolveInstance.");
        }

        switch (level)
        {
            case GH_RuntimeMessageLevel.Blank:
                break;
            case GH_RuntimeMessageLevel.Remark:
                _currentAccess.AddRemark(text, text);
                break;
            case GH_RuntimeMessageLevel.Warning:
                _currentAccess.AddWarning(text, text);
                break;
            case GH_RuntimeMessageLevel.Error:
                _currentAccess.AddError(text, text);
                break;
            default:
                throw new ArgumentException($"Unknown runtime message level: {level}");
        }
    }
    protected virtual void BeforeSolveInstance()
    {
    }
    protected virtual void AfterSolveInstance()
    {
    }
    protected override void PreProcess(Solution solution)
    {
        BeforeSolveInstance();
        base.PreProcess(solution);
    }
    protected override void PostProcess(Solution solution)
    {
        AfterSolveInstance();
        base.PostProcess(solution);
    }
    protected virtual GH_Exposure Exposure { get; set; }
    public string Message
    {
        get => Label;
        set => Label = value;
    }
}
