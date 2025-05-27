using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract partial class VariableParameterComponent<TComp> : ComponentMiddleware<TComp>, IGH_VariableParameterComponent
    where TComp : VariableParameterComponent<TComp>
{
    protected VariableParameterComponent(string name, string nickname, string desc, string category, string subcategory) :
        base(name, nickname, desc, category, subcategory)
    {
    }

    protected VariableParameterComponent(IReader reader) : base(reader) { }
    public abstract void VariableParameterMaintenance();
    public abstract bool CanInsertParameter(GH_ParameterSide side, int index);
    bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index) => CanRemoveParameter2(side, index);
    public abstract bool CanRemoveParameter2(GH_ParameterSide side, int index);
    public abstract IGH_Param CreateParameter(GH_ParameterSide side, int index);
    public abstract bool DestroyParameter(GH_ParameterSide side, int index);
}