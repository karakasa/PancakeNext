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

    private sealed class ComponentInfo
    {
        public Type Identifier { get; set; } = typeof(void);
        public CategoryInfo? Section { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int SubSection { get; set; }
        public Rank Rank { get; set; }
        public Nomen ToNomen(bool obsolete = false)
        {
            if (Identifier.Name.IndexOf(DeprecatedSuffix, StringComparison.Ordinal) > 0 || Identifier.GetCustomAttribute<ObsoleteAttribute>() is not null)
                obsolete = true;

            return new Nomen(Name, Description, CategoryName, Section?.ToString() ?? "", SubSection, obsolete ? Rank.Hidden : Rank);
        }

        public static implicit operator ComponentInfo((Type Identifier, string Name, string Desc, int SubSection) tuple)
        {
            return (tuple.Identifier, tuple.Name, tuple.Desc, tuple.SubSection, Rank.Normal);
        }

        public static implicit operator ComponentInfo((Type Identifier, string Name, string Desc, int SubSection, Rank Rank) tuple)
        {
            return new()
            {
                Identifier = tuple.Identifier,
                Name = tuple.Name,
                Description = tuple.Desc,
                SubSection = tuple.SubSection,
                Rank = tuple.Rank
            };
        }
    }

    private static readonly Dictionary<string, ParamInfo> _paramInfos = [];
    private static readonly Dictionary<Type, ComponentInfo> _componentInfos = [];

    static int LastCategoryIndex = -1;

    private static void AddCategory(string categoryName, IEnumerable<ComponentInfo> infos)
    {
        ++LastCategoryIndex;
        var c = new CategoryInfo(categoryName, LastCategoryIndex);

        foreach (var info in infos)
        {
            info.Section = c;
            _componentInfos[info.Identifier] = info;
        }
    }

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
        AddBuiltinComponentList();
        AddBuiltinParamList();
    }

    public const string CategoryName = "Pancake";
    public const string DeprecatedSuffix = "_old";
    public static Nomen LookUpComponent(Type name, bool obsolete = false)
    {
        return _componentInfos[name].ToNomen(obsolete);
    }
}
