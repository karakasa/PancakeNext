using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Rhino.UI;
using System;
using System.Collections;
using System.Xml.Linq;

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
        private IDataAccess Access => _comp._currentAccess!;

        public int Iteration => Access.Iterations;

        public void AbortComponentSolution() => Access.Solution.Cancelled();

        public bool BlitData<Q>(int paramIndex, GH_Structure<Q> tree, bool overwrite) where Q : IGH_Goo
        {
            throw new NotImplementedException();
        }

        public void DisableGapLogic()
        {
            throw new NotSupportedException();
        }

        public void DisableGapLogic(int paramIndex)
        {
            throw new NotSupportedException();
        }

        public bool GetData<T>(int index, ref T destination)
        {
            return Access.GetItem(index, out destination);
        }

        public bool GetData<T>(string name, ref T destination)
        {
            for (var i = 0; i < _comp.Parameters.InputCount; i++)
            {
                if (_comp.Parameters.Input(i).Nomen.Name == name)
                {
                    return GetData(i, ref destination);
                }
            }
            return false;
        }

        public bool GetDataList<T>(int index, List<T> list)
        {
            throw new NotImplementedException();
        }

        public bool GetDataList<T>(string name, List<T> list)
        {
            throw new NotImplementedException();
        }

        public bool GetDataTree<T>(int index, out GH_Structure<T> tree) where T : IGH_Goo
        {
            throw new NotImplementedException();
        }

        public bool GetDataTree<T>(string name, out GH_Structure<T> tree) where T : IGH_Goo
        {
            throw new NotImplementedException();
        }

        public void IncrementIteration()
        {
            throw new NotImplementedException();
        }

        public int ParameterTargetIndex(int paramIndex)
        {
            throw new NotImplementedException();
        }

        public GH_Path ParameterTargetPath(int paramIndex)
        {
            throw new NotImplementedException();
        }

        public bool SetData(int paramIndex, object data)
        {
            Access.SetItem(paramIndex, data);
            return true;
        }

        public bool SetData(int paramIndex, object data, int itemIndexOverride)
        {
            throw new NotSupportedException();
        }

        public bool SetData(string paramName, object data)
        {
            for (var i = 0; i < _comp.Parameters.OutputCount; i++)
            {
                if (_comp.Parameters.Output(i).Nomen.Name == paramName)
                {
                    return SetData(i, data);
                }
            }

            return false;
        }

        public bool SetDataList(int paramIndex, IEnumerable data)
        {
            throw new NotImplementedException();
        }

        public bool SetDataList(int paramIndex, IEnumerable data, int listIndexOverride)
        {
            throw new NotImplementedException();
        }

        public bool SetDataList(string paramName, IEnumerable data)
        {
            throw new NotImplementedException();
        }

        public bool SetDataTree(int paramIndex, IGH_DataTree tree)
        {
            throw new NotImplementedException();
        }

        public bool SetDataTree(int paramIndex, IGH_Structure tree)
        {
            throw new NotImplementedException();
        }

        public int Util_CountNonNullRefs<T>(List<T> L)
        {
            throw new NotImplementedException();
        }

        public int Util_CountNullRefs<T>(List<T> L)
        {
            throw new NotImplementedException();
        }

        public bool Util_EnsureNonNullCount<T>(List<T> L, int N)
        {
            throw new NotImplementedException();
        }

        public int Util_FirstNonNullItem<T>(List<T> L)
        {
            throw new NotImplementedException();
        }

        public List<T> Util_RemoveNullRefs<T>(List<T> L)
        {
            throw new NotImplementedException();
        }
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
