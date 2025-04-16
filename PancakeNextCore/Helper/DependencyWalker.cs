using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Grasshopper;
using Grasshopper.Kernel;
using Pancake.Dataset;
using Pancake.UI;
using Pancake.Utility;

namespace PancakeNextCore.Helper;

internal class DependencyWalker
{
    private static bool IsSubfolderOf(string parentPath, string childPath)
    {
        try
        {
            return Path.GetFullPath(childPath)
            .ToLowerInvariant()
            .StartsWith(Path.GetFullPath(parentPath)
            .ToLowerInvariant());
        }
        catch (Exception e)
        {
            return false;
        }
    }

    static DependencyWalker()
    {
        ComponentFolder = Path.Combine(Folders.AppDataFolder, "Libraries");
    }
    private static string ComponentFolder { get; set; }
    private static readonly string DllExtension = Config.IsMac ? ".so" : ".dll";

    public static bool SearchForPInvoke { get; set; } = true;

    public static IEnumerable<string> GetPInvokeReferences(Assembly assembly)
    {
        var location = assembly.Location;
        if (string.IsNullOrEmpty(location))
            location = ComponentFolder;
        else
            location = Path.GetDirectoryName(location);

        var caches = new HashSet<string>();

        try
        {
            foreach (var it in assembly.GetTypes())
            {
                try
                {
                    foreach (var method in it.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        try
                        {
                            if (!method.Attributes.HasFlag(MethodAttributes.PinvokeImpl))
                                continue;

                            var attr = method.GetCustomAttribute<DllImportAttribute>();
                            if (attr is null)
                                continue;

                            var dll = attr.Value;

                            if (caches.Add(dll))
                            {
                                if (!dll.EndsWith(DllExtension, StringComparison.OrdinalIgnoreCase))
                                    dll += DllExtension;

                                var filepath = Path.Combine(location, dll);
                                if (File.Exists(filepath))
                                    yield return filepath;
                            }
                        }
                        finally
                        {
                        }
                    }
                }
                finally
                {
                }
            }
        }
        finally
        {
        }
    }
    public static void WalkThroughAssemblyDependency(Assembly rootAssembly, HashSet<string> location, Func<string, bool> judge = null)
    {
        if (rootAssembly == null)
        {
            // No assembly for this object, likely due to the class is dynamically generated, such as *.ghpy files.
            return;
        }

        foreach (var rawAssemblyName in rootAssembly.GetReferencedAssemblies())
        {
            try
            {
                var rawAssembly = Assembly.Load(rawAssemblyName);
                if (rawAssembly == null || rawAssembly.Location == string.Empty)
                    continue;

                var sub = IsSubfolderOf(Folders.AppDataFolder, rawAssembly.Location);

                if (!sub && (judge == null || !judge(rawAssembly.Location)))
                    continue;

                if (location.Add(rawAssembly.Location))
                    WalkThroughAssemblyDependency(rawAssembly, location, judge);
            }
            catch (Exception)
            {
                // exception explicitly ignored
            }
        }

        if (SearchForPInvoke)
        {
            foreach (var file in GetPInvokeReferences(rootAssembly))
            {
                location.Add(file);
            }
        }
    }

    internal static void GetTargetVersion(Assembly assembly, out string ghVer, out string rhinoVer)
    {
        ghVer = "";
        rhinoVer = "";

        var collected = 0;

        try
        {
            if (assembly == null)
                return;
            var names = assembly.GetReferencedAssemblies();
            foreach (var it in names)
            {
                var name = it.Name;
                if (name.Equals("Grasshopper", StringComparison.OrdinalIgnoreCase))
                {
                    ghVer = it.Version.ToString();
                    collected++;
                }
                else if (name.Equals("RhinoCommon", StringComparison.OrdinalIgnoreCase))
                {
                    rhinoVer = it.Version.ToString();
                    collected++;
                }

                if (collected == 2)
                    break;
            }
        }
        catch
        {
            // Slient fail
        }
    }

    public static void ShowPluginManager()
    {
        if (PersistentEtoForm<UI.EtoForms.FormAddonManager>.TryShow())
            return;

        var names = new List<string>();
        var keys = new List<string>();
        var icons = new List<Eto.Drawing.Image>();
        var infos = new List<List<KeyValuePair<string, string>>>();
        var files = new List<List<KeyValuePair<string, string>>>();
        var objs = new List<List<KeyValuePair<string, string>>>();
        var append1 = new List<string>();
        var append2 = new List<string>();

        var libs = Instances.ComponentServer.Libraries;
        var order = -1;
        foreach (var lib in libs)
        {
            ++order;

            try
            {
                var name = "";
                var version = "";
                try
                {
                    name = lib.Name;
                    version = lib.Version;
                }
                catch
                {

                }

                if (string.IsNullOrEmpty(name) && lib.Location.ToLowerInvariant().Contains("ghpython.gha"))
                    name = "GhPython Placeholder";

                var searchKey = $"{name} {version}|{lib.AuthorName}|{lib.AuthorContact}|{lib.Description}";
                names.Add(name);

                keys.Add(searchKey);
                icons.Add(EtoAssemblyIconCacher.Instance.Get(lib));
                append1.Add(version);
                append2.Add(lib.AuthorName);

                var info = new List<KeyValuePair<string, string>>();

                AddToList(info, Strings.Name, name);
                AddToList(info, Strings.Version, version);
                AddToList(info, Strings.Location, lib.Location);
                AddToList(info, Strings.Description, lib.Description);
                AddToList(info, Strings.LicenseAddonMgr, Enum.GetName(typeof(GH_LibraryLicense), lib.License));

                AddToList(info, "", "");

                AddToList(info, Strings.Author, lib.AuthorName);
                AddToList(info, Strings.Contact, lib.AuthorContact);

                AddToList(info, "", "");

                var id = lib.Id;

                AddToList(info, Strings.Guid, id.ToString());
                AddToList(info, Strings.LoadingOrder, order.ToString());
                AddToList(info, Strings.LoadingDemand,
                    Enum.GetName(typeof(GH_LoadingDemand),
                    CentralSettings.GetLoadMechanism(Path.GetFileNameWithoutExtension(lib.Location))));
                AddToList(info, Strings.LoadingMechanism, Enum.GetName(typeof(GH_LoadingMechanism), lib.LoadingMechanism));
                AddToList(info, Strings.CoreLibrary, lib.IsCoreLibrary ? "true" : "false");
                AddToList(info, Strings.ShippedWithRhino, LibraryInfo.IsLibraryShippedWithRhino(lib) ? "true" : "false");

                GetTargetVersion(lib.Assembly, out var ghver, out var rhinover);
                AddToList(info, Strings.TargetGrasshopperVersion, ghver);
                AddToList(info, Strings.TargetRhinoCommonVersion, rhinover);

                if (lib.Location.ToLowerInvariant().Contains(@"\6\libraries\"))
                    AddToList(info, Strings.LoadHint, Strings.RhinoOnly);
                else
                {
                    AddToList(info, Strings.LoadHint,
                        File.Exists(Path.ChangeExtension(lib.Location, "no6")) ? Strings.Rhino5Only : Strings.Unspecific);
                }

                var loadblock = lib.LoadingMechanism == GH_LoadingMechanism.Direct ||
                                 lib.LoadingMechanism == GH_LoadingMechanism.COFF && !Unblock.CheckZoneIdentifier(lib.Location)
                    ? Strings.Found
                    : Strings.FoundAddonMgr;

                AddToList(info, Strings.LoadBlock, loadblock);

                infos.Add(info);

                var file = new List<KeyValuePair<string, string>>();
                if (File.Exists(lib.Location))
                {
                    AddToList(file, Strings.Host, lib.Location);
                }
                var assemblies = lib.Assembly?.GetReferencedAssemblies();
                if (assemblies == null || assemblies.Length <= 0)
                {
                    if (!lib.Location.EndsWith(".ghpy", StringComparison.InvariantCultureIgnoreCase))
                        AddToList(file, Strings.Error, Strings.CannotLoadReferencedFiles);
                }
                else
                {
                    try
                    {
                        var filelist = new HashSet<string>();

                        WalkThroughAssemblyDependency(lib.Assembly, filelist);

                        foreach (var it in filelist)
                            AddToList(file, Strings.Referenced, it);
                    }
                    catch (Exception ex)
                    {
                        // Dependency load fail.

                        AddToList(file, "Error", "Fail to analyze this plugin. " + ex.Message);
                    }
                }
                files.Add(file);

                var obj = new List<KeyValuePair<string, string>>();
                foreach (var it in Instances.ComponentServer.FindObjects(id))
                {
                    var objname = it.Desc.Name;
                    if (it.Exposure == GH_Exposure.hidden)
                        objname += Strings.Hidden;
                    AddToList(obj, objname, it.Type.FullName);
                }
                objs.Add(obj);
            }
            catch (Exception ex)
            {
                var name = "";
                try
                {
                    name = lib.Name;
                }
                catch
                {
                    name = "";
                }

                var searchKey = string.Format(Strings.DependencyWalker_ShowPluginManager__0__failed, name);
                names.Add(name);
                icons.Add(null);
                keys.Add(searchKey);
                append1.Add("");
                append2.Add("");

                var info = new List<KeyValuePair<string, string>>();

                AddToList(info, Strings.ProcessFail, "True");
                AddToList(info, "", "");
                AddToList(info, Strings.Type, ex.GetType().FullName);
                AddToList(info, Strings.Description, ex.Message);

                infos.Add(info);

                var file = new List<KeyValuePair<string, string>>();
                files.Add(file);

                var obj = new List<KeyValuePair<string, string>>();
                objs.Add(obj);
            }
        }

        Presenter.ShowPluginManagerWindow(names, keys, icons, infos, files, objs, append1, append2);
    }

    private static void AddToList<T1, T2>(List<KeyValuePair<T1, T2>> list, T1 a, T2 b)
    {
        list.Add(new KeyValuePair<T1, T2>(a, b));
    }
}
