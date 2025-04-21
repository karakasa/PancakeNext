using Eto.Drawing;
using Grasshopper2.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PancakeNextCore.Dataset;

internal static partial class ComponentLibrary
{
#if DEBUG
    private static HashSet<string> _queryed = [];

    public static IEnumerable<string> ParametersNotAccessed()
    {
        return _paramInfos.Keys.Except(_queryed);
    }
#endif

    public static bool LookupLocalizedParamInfo(string identifier, out string name, out string nickname, out string desc)
    {
        name = nickname = desc = null;

#if DEBUG
        _queryed.Add(identifier);
#endif

        if (!_paramInfos.TryGetValue(identifier, out var val))
        {
#if DEBUG
            // Error
#endif
            return false;
        }

        name = val.Name;
        nickname = val.NickName;
        desc = val.Description;

        return true;
    }

    private struct ParamInfo
    {
        public string Name;
        public string NickName;
        public string Description;
    };

    private sealed class CategoryInfo
    {
        public string FriendlyName { get; }
        public int Index { get; }

        readonly string Description;
        public CategoryInfo(string friendlyName, int index)
        {
            FriendlyName = friendlyName;
            Index = index;

            Description = $"{Index:D2} | {FriendlyName}";
        }
        public override string ToString() => Description;
    }

    private static readonly Dictionary<string, ParamInfo> _paramInfos = [];
    public static void Param(string identifier, string name, string nickname, string desc)
    {
        _paramInfos[identifier] = new()
        {
            Name = name,
            NickName = nickname,
            Description = desc
        };
    }

    static ComponentLibrary()
    {
        AddBuiltinParamList();
    }

    public const string PanelName = "Pancake";
    public const string DeprecatedSuffix = "_old";

    public static string GetCategoryFriendlyName(string categoryShortName)
    {
        return categoryShortName switch
        {
            "io" => "00 | IO",
            "assoc" or "data" => "01 | Association",
            "qty" => "02 | Quantity",
            "misc" => "03 | Misc",
            _ => throw new ArgumentOutOfRangeException(nameof(categoryShortName), $"{categoryShortName} is not a valid category shortname."),
        };
    }
}
