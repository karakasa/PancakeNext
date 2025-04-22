using Grasshopper2.Doc;
using PancakeNextCore.Dataset;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PancakeNextCore.Utility;

public static class LocalizationHelper
{
    private const string ConfigCheckLanguage = "CheckLanguageTraits";

    public static bool CheckLanguageTraitsAfterOpening
    {
        get => Config.Read(ConfigCheckLanguage, true, false);
        set => Config.Write(ConfigCheckLanguage, value.ToString());
    }

    public static void LocalizeNameAndDescrpition(ref string name, ref string desc)
    {
        var originalName = name;
        var originalDesc = desc;

        name = GetDisplayName(originalName);
        desc = GetDisplayDescription(originalName, originalDesc);
    }

    public static string GetDisplayName(string name)
    {
        if (GlobalizationResolver.IsNeutralLanguage)
        {
            return name;
        }
        else
        {
            var index = name.IndexOf(":");
            if (index == -1) return name;

            var neutralName = name.Substring(0, index).Trim();
            var localName = name.Substring(index + 1).Trim();

            return GlobalizationResolver.PreferNeutralName ? neutralName : localName;
        }
    }

    public static string GetDisplayDescription(string name, string desc)
    {
        if (GlobalizationResolver.IsNeutralLanguage)
        {
            return desc;
        }
        else
        {
            const string NamePair = "{0}\r\n{1}";

            var index = name.IndexOf(":");
            if (index == -1) return desc;

            if (GlobalizationResolver.PreferNeutralName)
            {
                var localName = name.Substring(index + 1).Trim();
                return string.Format(NamePair, localName, desc);
            }
            else
            {
                var neutralName = name.Substring(0, index).Trim();
                return string.Format(NamePair, neutralName, desc);
            }
        }
    }

    public static IEnumerable<IPancakeLocalizable> GetObjectsWithChangedLanguages(Document ghdoc)
    {
        var current = GlobalizationResolver.CurrentLanguageTraits;

        return ghdoc.Objects.Objects.OfType<IPancakeLocalizable>().Where(obj =>
        {
            var lang = obj.LastSaveLocalization ?? GlobalizationResolver.NeutralLanguageTraits;

            return current != lang;
        });
    }

    private static HashSet<Guid> _notifiedIds = new HashSet<Guid>();

    internal static IEnumerable<string> GetAvailableLanguageOptions()
    {
        foreach (var dir in Directory.EnumerateDirectories(
            GlobalizationResolver.PluginDirectory, "*-*", SearchOption.TopDirectoryOnly))
        {
            if (File.Exists(Path.Combine(dir, "Pancake.resources.dll")))
                yield return Path.GetFileName(dir);
        }
    }

    private static List<string> _availLangs = null;

    internal static IReadOnlyCollection<string> AvailableLangs
    {
        get
        {
            _availLangs ??= GetAvailableLanguageOptions().ToList();
            return _availLangs.AsReadOnly();
        }
    }

    internal static IEnumerable<string> AvailableLangsInclEng
    {
        get
        {
            yield return "en-US";
            foreach (var it in AvailableLangs) yield return it;
        }
    }
}
