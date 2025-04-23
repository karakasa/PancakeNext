using PancakeNextCore.Interfaces;
using System;

namespace PancakeNextCore.Modules.TransferSetting;

public abstract class Base : IModule, IModuleGuidCapable
{
    public const string ObjectiveIdentifier = "TransferSetting";

    public abstract Guid Guid { get; }
    public abstract string FriendlyName { get; }
    public abstract bool EnabledByDefault { get; }
    public abstract bool RestartRequired { get; }

    public abstract byte[] SaveToByteArary();
    public abstract bool LoadFromByteArray(byte[] setting);

    public abstract string Name { get; }
    public string Objective => ObjectiveIdentifier;
    public abstract bool InternalModule { get; }
    public Guid Identifier => Guid;

    public override string ToString()
    {
        return FriendlyName;
    }
}
