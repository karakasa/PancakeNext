using System;

namespace PancakeNextCore.Interfaces;

public interface IModuleGuidCapable
{
    Guid Identifier { get; }
}
