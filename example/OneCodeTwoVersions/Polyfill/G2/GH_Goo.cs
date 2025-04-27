using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract class GH_Goo<T> : IGH_Goo
{
    public virtual T Value { get; set; }

    public virtual bool IsValid => true;

    public virtual string IsValidWhyNot => IsValid ? string.Empty : $"This {TypeName} is not valid, but I don't know why.";

    public virtual string TypeName { get; private set; }

    public virtual string TypeDescription { get; private set; }
    protected GH_Goo()
    {
    }
    protected GH_Goo(string typeName, string? typeDesc = null)
    {
        TypeName = typeName;
        TypeDescription = typeDesc ?? typeName;
    }

    protected GH_Goo(T internal_data)
    {
        Value = internal_data;
    }

    protected GH_Goo(GH_Goo<T> other)
    {
        if (other == null)
        {
            throw new ArgumentNullException("other");
        }

        Value = other.Value;
    }

    public abstract IGH_Goo Duplicate();
    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }
    public virtual bool CastFrom(object source)
    {
        if (source is not null && source is T v)
        {
            Value = v;
            return true;
        }

        return false;
    }
    public virtual bool CastTo<Q>(out Q target)
    {
        if (typeof(T) == typeof(Q))
        {
            target = (Q)(object)Value;
            return true;
        }

        target = default;
        return false;
    }

    public virtual object ScriptVariable() => Value;
}