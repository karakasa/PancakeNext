using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract class GH_Param<TGoo, TGh2Parameter, TInner> : ParameterWrapper
    where TGh2Parameter : IParameter, new()
    where TGoo : GH_Goo<TInner>
{
    protected TGh2Parameter Value
    {
        get => (TGh2Parameter)_value;
        set => _value = value;
    }
    public GH_Param() : base(new TGh2Parameter())
    {
    }
    public GH_Param(TGh2Parameter p) : base(p)
    {
    }
}
public abstract class GH_Param<TGoo, TGh2Parameter, TGh1Inner, TGh2Inner> : ParameterWrapper
    where TGh2Parameter : IParameter, new()
    where TGoo : GH_Goo<TGh1Inner>
{
    protected TGh2Parameter Value
    {
        get => (TGh2Parameter)_value;
        set => _value = value;
    }
    public GH_Param() : base(new TGh2Parameter())
    {
    }
    public GH_Param(TGh2Parameter p) : base(p)
    {
    }
    protected abstract TGh1Inner To1(TGh2Inner goo);
    protected abstract TGh2Inner To2(TGh1Inner goo);
}
