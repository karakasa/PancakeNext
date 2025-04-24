using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if !NET

namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class MemberNotNullAttribute : Attribute
{
    public MemberNotNullAttribute(string member) { }
    public MemberNotNullAttribute(params string[] members) { }
}
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false, AllowMultiple = false)]
public sealed class NotNullIfNotNullAttribute : Attribute
{
    public NotNullIfNotNullAttribute(string member) { }
    public NotNullIfNotNullAttribute(params string[] members) { }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class MemberNotNullWhenAttribute : Attribute
{
    public MemberNotNullWhenAttribute(bool result, string member) { }
    public MemberNotNullWhenAttribute(bool result, params string[] members) { }
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class NotNullWhenAttribute : Attribute
{
    public NotNullWhenAttribute(bool result) { }
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DisallowNullAttribute : Attribute
{
    public DisallowNullAttribute() { }
}

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ConstantExpectedAttribute : Attribute{}

#endif