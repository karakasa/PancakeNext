﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using GrasshopperIO;
using OneCodeTwoVersions.Polyfill;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions;
[IoId("BF647C43-8B2F-49C6-84D3-708BCAF2F126")]
[RibbonPosition(1)]
public sealed class DemoVariableComponent : VariableParameterComponent<DemoVariableComponent>
{
    public DemoVariableComponent() : base("DemoVar", "DemoVar", "Demo variable component for Grasshopper 1 and 2", "Demo", "Demo")
    {
    }
    public DemoVariableComponent(IReader reader) : base(reader) { }
    public override bool CanInsertParameter(GH_ParameterSide side, int index)
    {
        return side == GH_ParameterSide.Input;
    }
    public override bool CanRemoveParameter(GH_ParameterSide side, int index)
    {
        return side == GH_ParameterSide.Input;
    }
    public override IGH_Param CreateParameter(GH_ParameterSide side, int index)
    {
        return new Param_Number();
    }

    public override bool DestroyParameter(GH_ParameterSide side, int index)
    {
        return true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter("Sum", "S", "Sum of inputs", GH_ParamAccess.item);
    }
    public override void VariableParameterMaintenance()
    {
        var cnt = Params.Input.Count;
        for (var i = 0; i < cnt; i++)
        {
            var p = Params.Input[i];
            p.Name = p.NickName = $"{i}";
        }
    }
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var sum = 0.0;
        var data = 0.0;
        var cnt = Params.Input.Count;
        for (var i = 0; i < cnt; i++)
        {
            DA.GetData(i, ref data);
            sum += data;
        }

        DA.SetData(0, sum);
    }
}
