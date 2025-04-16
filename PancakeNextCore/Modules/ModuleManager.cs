using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pancake.Interfaces;
using Pancake.Utility;

namespace PancakeNextCore.Modules;

public class ModuleManager<T> : IModuleManager where T : class, IModule
{
    protected readonly List<Type> _types = new List<Type>();
    protected readonly List<T> _createdInstances = new List<T>();

    protected readonly Dictionary<Guid, Type> _guidCache = new Dictionary<Guid, Type>();

    protected bool _typesLatest = false;

    internal ModuleManager()
    {
    }

    public void AddModuleType(Type moduleType)
    {
        _typesLatest = false;
        _types.Add(moduleType);
    }

    public void AddModule(T module)
    {
        AddModuleType(module.GetType());
        _createdInstances.Add(module);
    }

    public void AddModule(object obj)
    {
        if (!(obj is T module))
            throw new ArgumentException(nameof(obj));

        AddModuleType(module.GetType());
        _createdInstances.Add(module);
    }

    public void LoadTypesFrom(Assembly assembly = null)
    {
        _typesLatest = false;
        _types.AddRange(ReflectionHelper.RetrieveAssemblyTypes<T>(assembly));
    }

    public void LoadTypesAndCreate(Assembly assembly = null)
    {
        LoadTypesFrom(assembly);
        CreateInstancesIfNot();
    }

    public void CreateInstancesIfNot()
    {
        _createdInstances.AddRange(_types
            .Except(_createdInstances.Select(o => o.GetType()))
            .Select(t => (T)Activator.CreateInstance(t)));
    }

    protected IEnumerable<T> CreateInstancesInternal()
    {
        return _types.Select(type => (T)Activator.CreateInstance(type));
    }

    public void CreateInstances()
    {
        _createdInstances.AddRange(CreateInstancesInternal());
    }

    public void Clear()
    {
        ClearTypes();
        ClearInstances();
        _guidCache.Clear();
    }

    public void ClearTypes()
    {
        _typesLatest = false;
        _types.Clear();
    }

    protected void DisposeList(IEnumerable<T> list)
    {
        foreach (var it in list)
        {
            if (it is IDisposable dispose)
                dispose?.Dispose();
        }
    }

    public void ClearInstances()
    {
        DisposeList(_createdInstances);
        _createdInstances.Clear();
    }

    public IReadOnlyList<T> Instances => _createdInstances.AsReadOnly();
    public IEnumerable<IModule> InstancesAsModule => Instances;
    public IEnumerable<object> InstancesAsObject => Instances;
}

public class ModuleManagerGuidBased<T> : ModuleManager<T> where T : class, IModule, IModuleGuidCapable
{
    private void RefreshGuidCache()
    {
        var insts = CreateInstancesInternal().ToList();
        _guidCache.Clear();
        foreach (var it in insts)
        {
            _guidCache[it.Identifier] = it.GetType();
        }
        DisposeList(insts);
        insts.Clear();
        _typesLatest = true;
    }

    public T CreateByGuid(Guid guid)
    {
        if (!_typesLatest) RefreshGuidCache();
        if (!_guidCache.TryGetValue(guid, out var type)) return null;

        return (T)Activator.CreateInstance(type);
    }

    public T RetrieveByGuid(Guid guid, bool createdIfNotExist = false)
    {
        if (!_typesLatest) RefreshGuidCache();

        var ins = _createdInstances.FirstOrDefault(inst => inst.Identifier == guid);
        if (ins == null)
        {
            if (createdIfNotExist)
            {
                var created = CreateByGuid(guid);
                if (created != null)
                    _createdInstances.Add(created);
                return created;
            }
            else
            {
                return null;
            }
        }

        return ins;
    }
}

public class ModuleManagerTypeBased<T> : ModuleManager<T> where T : class, IModule, IModuleTypeCapable
{
    public bool AllowDynamicDispatch = true;
    public ModuleUtility.TypeLookupMode ProcessingMode = ModuleUtility.TypeLookupMode.FirstMatch;

    public IEnumerable<T> LookupSupportedInstance(Type operand)
    {
        foreach (var it in _createdInstances)
        {
            if (it.ApplicableType.Equals(operand)
                || AllowDynamicDispatch && it.CanProcessType(operand))
                yield return it;
        }
    }

    public IEnumerable<T> LookupSupportedInstance<T2>()
    {
        var operand = typeof(T2);

        foreach (var it in _createdInstances)
        {
            if (it.ApplicableType.Equals(operand)
                || AllowDynamicDispatch && it.CanProcessType(operand))
                yield return it;
        }
    }

    public T1 Process<T1>(object obj) => (T1)Process(obj);

    public object Process(object obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        var type = obj.GetType();
        CreateInstancesIfNot();

        if (ProcessingMode == ModuleUtility.TypeLookupMode.FirstMatch)
        {
            var instance = LookupSupportedInstance(type).FirstOrDefault();
            if (instance == null) return null;

            return instance.Process(obj);
        }
        else
        {
            var instances = LookupSupportedInstance(type).OrderByDescending(o => o.Priority);
            var curObj = obj;
            foreach (var it in instances)
            {
                curObj = it.Process(curObj);
            }

            return curObj;
        }
    }

    public T2 ProcessFirstMatch<T2, T1>(T1 obj)
    {
        CreateInstancesIfNot();
        var instance = LookupSupportedInstance(typeof(T1)).FirstOrDefault();
        if (instance == null) return default;

        if (instance is IModuleTypeCapable<T2, T1> imdl)
            return imdl.ProcessGeneric(obj);

        return (T2)instance.Process(obj);
    }

    public T1 ProcessChain<T1>(T1 obj)
    {
        CreateInstancesIfNot();
        var instances = LookupSupportedInstance(typeof(T1)).OrderByDescending(o => o.Priority);
        var curObj = obj;
        foreach (var it in instances)
        {
            if (it is IModuleTypeCapable<T1, T1> imdl)
                curObj = imdl.ProcessGeneric(obj);
            else
                curObj = (T1)it.Process(obj);
        }

        return curObj;
    }
}
