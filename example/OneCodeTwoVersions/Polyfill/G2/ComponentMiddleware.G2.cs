#if G2
using Grasshopper2.Components;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Grasshopper2.UI;
using GrasshopperIO;

namespace OneCodeTwoVersions.Polyfill;
public abstract partial class ComponentMiddleware<TComp> : Component, IGH_InstanceDescription
    where TComp : ComponentMiddleware<TComp>
{
    protected ComponentMiddleware(string name, string nickname, string desc, string category, string subcategory) : base(CreateNomen(name, nickname, desc, category, subcategory))
    {
    }

    protected ComponentMiddleware(IReader reader) : base(reader) { }

    private static Nomen CreateNomen(string name, string nickname, string desc, string category, string subcategory)
    {
        return new Nomen(name, desc, category, subcategory, slot: 0, rank: Rank.Normal, null);
    }

    // Placeholder
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

    private IDataAccess? _currentAccess;

    protected override void Process(IDataAccess access)
    {
        try
        {
            _currentAccess = access;
            SolveInstance(new DataAccessWrapper(this));
        }
        finally
        {
            _currentAccess = null;
        }
    }
    public virtual void AddRuntimeMessage(GH_RuntimeMessageLevel level, string text)
    {
        if (_currentAccess is null) return;

        switch (level)
        {
            case GH_RuntimeMessageLevel.Remark:
                _currentAccess.AddRemark("Remark", text);
                break;
            case GH_RuntimeMessageLevel.Warning:
                _currentAccess.AddWarning("Warning", text);
                break;
            case GH_RuntimeMessageLevel.Error:
                _currentAccess.AddError("Error", text);
                break;
        }
    }
}

#endif