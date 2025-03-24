using Rhino;
using System;

namespace PancakeNextCore.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public abstract class MinimalVersionAttribute : Attribute
{
    public abstract bool SatisfyRequirement();
    public abstract string GetAnticipatedVersion();
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class MinimalRhinoVersionAttribute : MinimalVersionAttribute
{
    static readonly int RhinoMajorVersion = RhinoApp.ExeVersion;
    static readonly Version RhinoVersion = RhinoApp.Version;

    private readonly int _rhinoMajorVersion;
    private readonly Version? _rhinoVersion;

    public MinimalRhinoVersionAttribute(int rhinoVersion)
    {
        this._rhinoMajorVersion = rhinoVersion;
    }

    public MinimalRhinoVersionAttribute(Version rhinoVersion)
    {
        this._rhinoVersion = rhinoVersion;
    }

    public override bool SatisfyRequirement()
    {
        if (_rhinoVersion is not null)
        {
            return RhinoVersion >= _rhinoVersion;
        }
        else
        {
            return RhinoMajorVersion >= _rhinoMajorVersion;
        }
    }

    public override string GetAnticipatedVersion()
    {
        if (_rhinoVersion is not null)
        {
            return $"Rhino {_rhinoVersion}";
        }
        else
        {
            return $"Rhino {_rhinoMajorVersion}";
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class MinimalGhVersionAttribute(Version ghVersion) : MinimalVersionAttribute
{
    static readonly Version GrasshopperVersion = Grasshopper2.Instances.FileVersion;
    public override bool SatisfyRequirement() => GrasshopperVersion >= ghVersion;
    public override string GetAnticipatedVersion()
    {
        return $"Grasshopper {GrasshopperVersion}";
    }
}
