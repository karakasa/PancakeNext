using Pancake.Interfaces;
using System;
using System.Collections.Generic;

namespace PancakeNextCore.Modules;

public static class ModuleUtility
{
    private static Dictionary<string, IModuleManager> _managers =
        new Dictionary<string, IModuleManager>();

    public static ModuleManager<T> GetManager<T>(string name, bool createInstance = false)
        where T : class, IModule
    {
        if (_managers.TryGetValue(name, out var imanager))
        {
            try
            {
                return (ModuleManager<T>)imanager;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(nameof(name));
            }
        }

        var manager = new ModuleManager<T>();
        if (createInstance)
            manager.LoadTypesAndCreate();

        _managers[name] = manager;
        return manager;
    }

    public enum TypeLookupMode
    {
        FirstMatch,
        Chain
    }

    public static ModuleManagerTypeBased<T> GetManagerTypeBased<T>(
        string name, TypeLookupMode mode = TypeLookupMode.FirstMatch,
        bool createInstance = false) where T : class, IModule, IModuleTypeCapable
    {
        if (_managers.TryGetValue(name, out var imanager))
        {
            try
            {
                return (ModuleManagerTypeBased<T>)imanager;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(nameof(name));
            }
        }

        var manager = new ModuleManagerTypeBased<T>();
        manager.ProcessingMode = mode;
        if (createInstance)
            manager.LoadTypesAndCreate();

        _managers[name] = manager;
        return manager;
    }

    public static ModuleManagerGuidBased<T> GetManagerGuidBased<T>(string name, bool createInstance = false) where T : class, IModule, IModuleGuidCapable
    {
        if (_managers.TryGetValue(name, out var imanager))
        {
            try
            {
                return (ModuleManagerGuidBased<T>)imanager;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(nameof(name));
            }
        }

        var manager = new ModuleManagerGuidBased<T>();
        if (createInstance)
            manager.LoadTypesAndCreate();

        _managers[name] = manager;
        return manager;
    }
}
