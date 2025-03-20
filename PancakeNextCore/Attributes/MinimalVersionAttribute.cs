using Rhino;
using System;

namespace PancakeNextCore.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class MinimalVersionAttribute : Attribute
{
    private readonly int _rhinoVer;

    public MinimalVersionAttribute(int rhinoVersion)
    {
        _rhinoVer = rhinoVersion;
    }

    public bool SatisfyRequirement()
    {
        return RhinoApp.ExeVersion >= _rhinoVer;
    }
}
