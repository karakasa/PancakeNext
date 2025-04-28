using GrasshopperIO;
using OneCodeTwoVersions.Polyfill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions;
[IoId("BF647C43-8B2F-49C6-84D3-708BCAF2F126")]
public sealed class DemoVariableComponent : VariableParameterComponent<DemoVariableComponent>
{
    public override bool CanInsertParameter(GH_ParameterSide side, int index)
    {
        throw new NotImplementedException();
    }

    public override bool CanRemoveParameter(GH_ParameterSide side, int index)
    {
        return false;
    }

    public override IGH_Param CreateParameter(GH_ParameterSide side, int index)
    {
        throw new NotImplementedException();
    }

    public override bool DestroyParameter(GH_ParameterSide side, int index)
    {
        throw new NotImplementedException();
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        throw new NotImplementedException();
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        throw new NotImplementedException();
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        throw new NotImplementedException();
    }
}
