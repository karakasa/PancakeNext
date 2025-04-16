//#define EnableShapeDiverSupport

using Grasshopper;
using Grasshopper.Kernel;
using Pancake.Dataset;
using Pancake.Interfaces;
using Pancake.Modules.PortabilityChecker;
using Pancake.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PancakeNextCore.Helper;

public static class PortabilityReport
{
#if EnableShapeDiverSupport
    internal const bool EnableShapediverSupport = false;
#endif
    public static string DefaultSystemConfiguration => Rhino.RhinoApp.ExeVersion switch
    {
        5 => Config.IsMac ?
            Rhino5WinTargetConfiguration.Version : Rhino5MacTargetConfiguration.Version,
        6 => Rhino6TargetConfiguration.Version,
        7 or 8 => Rhino7TargetConfiguration.Version,
        _ => null,
    };

    public static string GetDefaultConfiguration(GH_Document doc)
    {
#if EnableShapeDiverSupport
        if (doc != null && EnableShapediverSupport)
        {
            foreach (var it in doc.Objects)
            {
                var guid = it.ComponentGuid;
                if (guid == ShapediverPresets.GuidDataOutput
                    || guid == ShapediverPresets.GuidDisplayGeometry
                    || guid == ShapediverPresets.GuidExportComponentEmail)
                    return ShapediverPresets.Identifier;
            }
        }
#endif


        return DefaultSystemConfiguration;
    }

    public static IPortabilityCheckerConfiguration DefaultChecker
    {
        get
        {
            if (TryGetConfiguration(DefaultSystemConfiguration, out var it))
                return it;
            return null;
        }
    }

    private static Dictionary<string, IPortabilityCheckerConfiguration> _configs = new Dictionary<string, IPortabilityCheckerConfiguration>();

    public static bool TryGetConfiguration(string name, out IPortabilityCheckerConfiguration config)
    {
        return _configs.TryGetValue(name, out config);
    }

    public static void AddConfiguration(IPortabilityCheckerConfiguration config)
    {
        _configs[config.Name] = config;
    }

    public static IEnumerable<string> AllConfigurations => _configs.Where(c => !c.Value.Hidden).Select(c => c.Key);
    static PortabilityReport()
    {
        foreach (var it in ReflectionHelper.GetEnumerableOfType<IPortabilityCheckerConfiguration>())
        {
            AddConfiguration(it);
        }
    }

    //public static string GenerateDefaultPortabilityReport()
    //{
    //    var checker = DefaultChecker;
    //    var doc = Instances.ActiveCanvas?.Document;
    //    if (checker is null || doc is null)
    //        return string.Empty;

    //    var results = checker.Checkers
    //        .SelectMany(chkr => chkr.AnalyzeDocument(doc))
    //        .OrderBy(entry => entry.Section ?? string.Empty)
    //        .GroupBy(entry => entry.Section ?? string.Empty);

    //    var sb = new StringBuilder();

    //    foreach (var grp in results)
    //    {
    //        sb.AppendLine(grp.Key);
    //        foreach (var it in grp
    //            .OrderBy(entry => entry.Name ?? string.Empty)
    //            .ThenBy(entry => entry.SubNameOverride ?? string.Empty))
    //        {
    //            sb.Append("    ");
    //            sb.Append(it.Name);
    //            sb.Append(": ");
    //            sb.AppendLine(it.ToString());
    //        }
    //    }

    //    if (sb.Length == 0)
    //        return Strings.NoIssuesAreDetected;
    //    else
    //        return sb.ToString();
    //}
}
