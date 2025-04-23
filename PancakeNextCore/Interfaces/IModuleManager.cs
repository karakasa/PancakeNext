using System;
using System.Collections.Generic;
using System.Reflection;

namespace PancakeNextCore.Interfaces;

public interface IModuleManager
{
    void AddModuleType(Type moduleType);
    void AddModule(object obj);
    void LoadTypesFrom(Assembly assembly = null);
    void CreateInstances();
    void Clear();
    void ClearTypes();
    void ClearInstances();
    IEnumerable<IModule> InstancesAsModule { get; }
    IEnumerable<object> InstancesAsObject { get; }
    void LoadTypesAndCreate(Assembly assembly = null);
}
