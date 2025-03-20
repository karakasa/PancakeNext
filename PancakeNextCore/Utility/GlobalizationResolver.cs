using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Grasshopper2;
using PancakeNextCore.Dataset;

namespace PancakeNextCore.Utility;

internal class GlobalizationResolver
{
    private static string _directory = null;
    private const string Resource = "Pancake.resources";
    private static Assembly _resourceAssembly;

    internal static string LanguageOverride
    {
        get => Config.Read("LangOverride", string.Empty, string.Empty);
        set => Config.Write("LangOverride", value);
    }

    public static bool IsLanguageOverrideNeutral
    {
        get => string.IsNullOrEmpty(LanguageOverride);
    }

    internal static void ProcessUICulture()
    {
        var language = LanguageOverride;
        if (!string.IsNullOrEmpty(language))
        {
            try
            {
                var info = CultureInfo.CreateSpecificCulture(language);
                if (info != default)
                    Thread.CurrentThread.CurrentUICulture = info;
            }
            catch (Exception ex)
            {

            }
        }
    }

    internal static string PluginDirectory
    {
        get
        {
            if (!string.IsNullOrEmpty(_directory))
                return _directory;

            return null;

            //foreach (var lib in Instances.ComponentServer.Libraries)
            //if (lib.Name == "Pancake")
            //{
            //_directory = Path.GetDirectoryName(lib.Location);
            //break;
            //}
            //return _directory;
        }
    }

    internal static string ResolveLanguage()
    {
        var lang = Config.Read("LangOverride", "", "en-US");
        if (!string.IsNullOrEmpty(lang))
            return lang;

        var loop = 0;
        var culture = CultureInfo.CurrentUICulture;
        while (!culture.Equals(CultureInfo.InvariantCulture))
        {
            if (File.Exists(CombineResourceFileName(culture.Name)))
                return culture.Name;
            culture = culture.Parent;
            loop++;
            if (loop > 10)
                break;
        }

        return null;
    }

    internal static string CombineResourceFileName(string lang) =>
        PluginDirectory + Path.DirectorySeparatorChar + lang + Path.DirectorySeparatorChar + Resource + ".dll";

    internal static Assembly ResourceResolver(object sender, ResolveEventArgs args)
    {
        if (args.RequestingAssembly != typeof(GlobalizationResolver).Assembly)
            return null;

        if (!args.Name.StartsWith(Resource)) return null;

        if (_resourceAssembly != null)
            return _resourceAssembly;

        if (string.IsNullOrEmpty(PluginDirectory))
            return null;
        var lang = ResolveLanguage();
        if (lang == "en-US")
            return null;

        if (string.IsNullOrEmpty(lang))
            return null;

        try
        {
            _resourceAssembly = Assembly.LoadFrom(CombineResourceFileName(lang));
            return _resourceAssembly;
        }
        catch
        {
            return null;
        }
    }

    public static string CurrentLanguageTraits => "neutral" + (PreferNeutralName ? ",N" : "");

    public static bool IsLanguageTraitsSame(string a, string b)
    {
        var la = a.Split(',');
        var lb = b.Split(',');
        if (la.Length == 0 && lb.Length == 0) return true;
        if (la.Length == 0 || lb.Length == 0) return false;

        if (la[0] == NeutralLanguageTraits && lb[0] == NeutralLanguageTraits) return true;

        if (la.Length != lb.Length) return false;
        if (la[0] != lb[0]) return false;
        Array.Sort(la);
        Array.Sort(lb);

        return !la.Zip(lb, (x, y) => x != y).Any(e => e);
    }

    public const string NeutralLanguageTraits = "neutral";

    public static bool IsNeutralLanguage => CurrentLanguageTraits == NeutralLanguageTraits;

    public static bool IsChineseCharacterSpecialized
    {
        get
        {
            var name = CultureInfo.CurrentUICulture.Name;
            return name == "ja-JP" || name.StartsWith("zh-", StringComparison.Ordinal);
        }
    }

    public static bool PreferNeutralName
    {
        get => Config.Read("PreferNeutralName", true, true);
        set => Config.Write("PreferNeutralName", value.ToString());
    }
}
