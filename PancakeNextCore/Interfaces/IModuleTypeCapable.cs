using System;

namespace PancakeNextCore.Interfaces;

public interface IModuleTypeCapable
{
    Type ApplicableType { get; }
    int Priority { get; }
    bool CanProcessType(Type type);
    object Process(object obj);
}

public interface IModuleTypeCapable<ResultType, ParamType>
{
    ResultType ProcessGeneric(ParamType obj);
}
