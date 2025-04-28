using Grasshopper2.Components;
using GrasshopperIO;
using Rhino.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract partial class VariableParameterComponent<TComp> : ComponentMiddleware<TComp>
    where TComp : VariableParameterComponent<TComp>
{
    protected VariableParameterComponent(string name, string nickname, string desc, string category, string subcategory) :
        base(name, nickname, desc, category, subcategory)
    {
    }

    protected VariableParameterComponent(IReader reader) : base(reader) { }
    public override bool CanCreateParameter(Side side, int index) => CanInsertParameter(side.To1(), index);
    public override bool CanRemoveParameter(Side side, int index) => CanRemoveParameter(side.To1(), index);
    public override void DoCreateParameter(Side side, int index)
    {
        var param = CreateParameter(side.To1(), index).UnderlyingObject;
        if (side == Side.Input)
            Parameters.AddInput(param, index);
        else
            Parameters.AddOutput(param, index);
    }
    public override void DoRemoveParameter(Side side, int index)
    {
        DestroyParameter(side.To1(), index);

        base.DoRemoveParameter(side, index);
    }
    public override void VariableParameterMaintenance()
    {
    }
    public abstract bool CanInsertParameter(GH_ParameterSide side, int index);
    public abstract bool CanRemoveParameter(GH_ParameterSide side, int index);
    public abstract IGH_Param CreateParameter(GH_ParameterSide side, int index);
    public abstract bool DestroyParameter(GH_ParameterSide side, int index);
}