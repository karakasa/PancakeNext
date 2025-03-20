using Grasshopper2.UI;
using System.Collections.Generic;
using System.Linq;

namespace PancakeNext.Dataset;

public static partial class ComponentLibrary
{
    public static bool IgnoreObsecure = true;

    public static string GetCategoryString(string section)
    {
        if (!_cateInfos.TryGetValue(section, out var v))
            return section;

        return $"{v.Index:D2} | {v.FriendlyName}";
    }

#if DEBUG
    private static HashSet<string> _queryed = new HashSet<string>();

    public static IEnumerable<string> NonQuery()
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

    private struct CategoryInfo
    {
        public string FriendlyName;
        public int Index;
    }

    private static Dictionary<string, ParamInfo> _paramInfos = new Dictionary<string, ParamInfo>();
    private static Dictionary<string, CategoryInfo> _cateInfos = new Dictionary<string, CategoryInfo>();

    public static void Param(string identifier, string name, string nickname, string desc)
    {
        _paramInfos[identifier] = new ParamInfo()
        {
            Name = name,
            NickName = nickname,
            Description = desc
        };
    }

    public static void Category(string identifier, string name, int index)
    {
        _cateInfos[identifier] = new CategoryInfo()
        {
            FriendlyName = name,
            Index = index
        };
    }

    static ComponentLibrary()
    {
        AddBuiltinParamList();
        AddBuiltinCategoryList();
    }
}
