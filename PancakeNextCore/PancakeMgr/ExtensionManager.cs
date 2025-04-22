using PancakeNextCore.Dataset;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PancakeNextCore.PancakeMgr;

/// <summary>
/// This class is to tackle Pancake-series plugins that may work without Pancake,
/// and send event to them. Plugins requiring Pancake should use Pancake as
/// an dependency.
/// </summary>
public class ExtensionManager
{
    public static ExtensionManager DefaultManager { get; } = new();

    private readonly List<Assembly> _extensionAssemblies;
    private readonly List<Type> _extensionTypes;

    private bool IsPancakeExtensionType(Type t) => t.Name == "PancakeNextExtension"
        && t.IsAbstract && t.IsSealed && t.IsClass;

    private bool IsPancakeExtension(Assembly assembly) => assembly.ExportedTypes
        .Any(IsPancakeExtensionType);

    private IEnumerable<Assembly> EnumerateExtensions() =>
        AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
        {
            var name = assembly?.GetName()?.Name;
            if (name == null || !name.StartsWith("Pancake"))
                return false;

            return IsPancakeExtension(assembly!);
        });

    public ExtensionManager()
    {
        if (Config.SafeMode)
        {
            _extensionAssemblies = [];
            _extensionTypes = [];
        }
        else
        {
            _extensionAssemblies = EnumerateExtensions().ToList();
            _extensionTypes = _extensionAssemblies
                .Select(a => a.ExportedTypes.FirstOrDefault(IsPancakeExtensionType)).ExcludeNulls().ToList();
        }
    }

    public void TriggerEvent(string eventName, params object[] parameters)
    {
        foreach (var type in _extensionTypes)
        {
            try
            {
                var method = type.GetMethod(eventName, BindingFlags.Public | BindingFlags.Static);
                method?.Invoke(null, parameters);
            }
            catch (Exception ex)
            {

            }
        }
    }

    public static void TriggerDefaultEvent(string eventName, params object[] parameters)
    {
        if (Config.SafeMode)
            return;

        DefaultManager?.TriggerEvent(eventName, parameters);
    }
}
