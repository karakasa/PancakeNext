using PancakeNextCore.Dataset;
using PancakeNextCore.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PancakeNextCore.PancakeMgr;

public sealed class FeatureManager
{
    private List<Feature> _features = [];

    public IReadOnlyCollection<Feature> LoadedFeatures => _features?.AsReadOnly();
    public void LoadFeatures(Assembly? assembly = null)
    {
        if (Config.SafeMode)
            return;
        _features.AddRange(ReflectionHelper.GetEnumerableOfType<Feature>(assembly).ToList());
    }

    public enum LoadStage
    {
        PreUI,
        UI
    }

    private string GetConfigEntry(string name)
    {
        return $"feature_{name}";
    }

    public void ProcessStartupHook(LoadStage stage)
    {
        foreach (var feature in _features)
        {
            if (!Config.Read(GetConfigEntry(feature.GetName()), feature.IsDefaultEnabled(), false)) continue;
            if (feature.GetExpectedLoadStage() == stage)
                feature.OnLoad();
        }
    }

    public T GetFeature<T>() where T : Feature
    {
        return _features.OfType<T>().FirstOrDefault();
    }
    public Feature GetFeatureByName(string featureName)
    {
        return _features.Where(f => f.GetName() == featureName).FirstOrDefault();
    }
    public T GetFeatureByName<T>(string featureName)
    {
        return _features.Where(f => f.GetName() == featureName).OfType<T>().FirstOrDefault();
    }

    public bool IsEffective<T>() where T : Feature
    {
        var feature = GetFeature<T>();
        if (feature == null) return false;

        return feature.IsEffective();
    }

    public bool IsEffective(string featureName)
    {
        var feature = GetFeatureByName(featureName);
        if (feature == null) return false;

        return feature.IsEffective();
    }

    public bool SetStatus(string featureName, bool newStatus, bool persistent = true)
    {
        var feature = GetFeatureByName(featureName);
        if (feature == null)
            return false;

        if (newStatus && !feature.IsEffective())
        {
            feature.OnLoad();
        }
        else if (!newStatus && feature.IsEffective())
        {
            feature.OnUnload();
        }

        if (persistent)
        {
            Config.Write(GetConfigEntry(feature.GetName()), newStatus.ToString());
        }

        return newStatus == feature.IsEffective();
    }
}
