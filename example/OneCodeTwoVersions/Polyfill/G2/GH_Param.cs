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
/// <summary>
/// Same underlying type
/// </summary>
/// <typeparam name="TGoo"></typeparam>
/// <typeparam name="TGh2Parameter"></typeparam>
/// <typeparam name="TGh1Inner"></typeparam>
public abstract class GH_Param<TGoo, TGh2Parameter, TGh1Inner> : ParameterWrapper
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
    public void SetPersistentData(params object[] values)
    {
        SetPersistentData(values.Cast<TGh1Inner>());
    }
    protected Parameter<TGh1Inner> StronglyTypedValue
    {
        get
        {
            if (Value is Parameter<TGh1Inner> param) return param;
            throw new NotSupportedException($"The underlying type is not Parameter<{typeof(TGh1Inner).Name}>.");
        }
    }
    public void SetPersistentData(TGh1Inner item)
    {
        StronglyTypedValue.Set([item]);
    }
    public void SetPersistentData(IEnumerable<TGh1Inner> items)
    {
        StronglyTypedValue.Set([.. items]);
    }
    public void SetPersistentData(GH_Structure<TGoo> items)
    {
        var p = StronglyTypedValue;
        var str = items.To2(false) as Tree<TGh1Inner> ??
            throw new NotSupportedException($"{typeof(TGoo).Name} is not represented by {typeof(TGh1Inner)}. Try to use the accurate type.");
        p.Set(str);
    }
}
/// <summary>
/// Different underlying type
/// </summary>
/// <typeparam name="TGoo"></typeparam>
/// <typeparam name="TGh2Parameter"></typeparam>
/// <typeparam name="TGh1Inner"></typeparam>
/// <typeparam name="TGh2Inner"></typeparam>
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
    public void SetPersistentData(params object[] values)
    {
        SetPersistentData(values.Cast<TGh1Inner>());
    }
    protected Parameter<TGh2Inner> StronglyTypedValue
    {
        get
        {
            if (Value is Parameter<TGh2Inner> param) return param;
            throw new NotSupportedException($"The underlying type is not Parameter<{typeof(TGh2Inner).Name}>.");
        }
    }
    public void SetPersistentData(TGh1Inner item)
    {
        StronglyTypedValue.Set([To2(item)]);
    }
    public void SetPersistentData(IEnumerable<TGh1Inner> items)
    {
        StronglyTypedValue.Set([.. items.Select(To2)]);
    }
    public void SetPersistentData(GH_Structure<TGoo> items)
    {
        var str = items.To2<TGoo, TGh1Inner, TGh2Inner>(To2, false) as Tree<TGh2Inner> ??
            throw new NotSupportedException($"{typeof(TGoo).Name} is not represented by {typeof(TGh2Inner)}. Try to use the accurate type.");
        StronglyTypedValue.Set(str);
    }
    protected abstract TGh1Inner To1(TGh2Inner goo);
    protected abstract TGh2Inner To2(TGh1Inner goo);
}
