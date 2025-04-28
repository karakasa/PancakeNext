using Grasshopper2.Data;
using Grasshopper2.Parameters;

namespace OneCodeTwoVersions.Polyfill;

public abstract class GH_PersistentParam<TGoo, TGh2Parameter, TInner> : GH_Param<TGoo, TGh2Parameter, TInner>
    where TGh2Parameter : Parameter<TInner>, new()
    where TGoo : GH_Goo<TInner>
{
    public GH_PersistentParam() : base(new TGh2Parameter())
    {
    }
    public GH_PersistentParam(TGh2Parameter p) : base(p)
    {
    }
    public void SetPersistentData(params object[] values)
    {
        SetPersistentData(values.Cast<TInner>());
    }
    protected Parameter<TInner> StronglyTypedValue
    {
        get
        {
            if (Value is Parameter<TInner> param) return param;
            throw new NotSupportedException($"The underlying type is not Parameter<{typeof(TInner).Name}>.");
        }
    }
    public void SetPersistentData(TInner item)
    {
        StronglyTypedValue.Set([item]);
    }
    public void SetPersistentData(IEnumerable<TInner> items)
    {
        StronglyTypedValue.Set([.. items]);
    }
    public void SetPersistentData(GH_Structure<TGoo> items)
    {
        var p = StronglyTypedValue;
        var str = items.To2(false) as Tree<TInner> ??
            throw new NotSupportedException($"{typeof(TGoo).Name} is not represented by {typeof(TInner)}. Try to use the accurate type.");
        p.Set(str);
    }
}

public abstract class GH_PersistentParam<TGoo, TGh2Parameter, TGh1Inner, TGh2Inner> : GH_Param<TGoo, TGh2Parameter, TGh1Inner, TGh2Inner>
    where TGh2Parameter : Parameter<TGh2Inner>, new()
    where TGoo : GH_Goo<TGh1Inner>
{
    public GH_PersistentParam() : base(new TGh2Parameter())
    {
    }
    public GH_PersistentParam(TGh2Parameter p) : base(p)
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
}
