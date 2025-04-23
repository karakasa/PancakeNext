using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using PancakeNextCore.Modules;
using PancakeNextCore.Modules.TransferSetting;
using PancakeNextCore.UI;
using Rhino;

namespace PancakeNextCore.Helper;

internal static class TransferSetting
{
    public static bool QuietMode { get; private set; }

    public static void LoadAndExit(string packedcfg)
    {
        QuietMode = true;
        LoadFromFile(packedcfg, Providers.Instances);
        RhinoApp.Exit();
    }

    private static ModuleManager<Base> Providers;
    static TransferSetting()
    {
        Providers = ModuleUtility.GetManagerGuidBased<Base>("transfersetting", true);
    }

    internal static void ShowTransferSettingUi()
    {
        const string extension = "pancake-packedcfg";
        const string extensionHint = $"*.{extension}|*.{extension}";

        var result = Presenter.ShowTransferWindow(Providers.Instances, out var selected);
        if (selected == null)
            return;
        switch (result)
        {
            case Presenter.TransferWindowResult.Cancelled:
                return;
            case Presenter.TransferWindowResult.SaveToFile:
                var path = UiHelper.SaveFileDialog(Strings.SavePackedSettings, extensionHint);
                if (!string.IsNullOrEmpty(path))
                {
                    var transferResult = SaveToFile(path, selected);
                    switch (transferResult)
                    {
                        case TransferResult.Succeeded:
                            UiHelper.Information(Strings.SettingsSuccesfullySaved);
                            break;
                        case TransferResult.Failed:
                            UiHelper.Information(Strings.CannotSaveSettings);
                            break;
                    }
                }

                break;
            case Presenter.TransferWindowResult.LoadFromFile:
                path = UiHelper.OpenFileDialog(Strings.OpenPackedSettings, extensionHint);
                if (!string.IsNullOrEmpty(path))
                {
                    var transferResult = LoadFromFile(path, selected,
                        () => UiHelper.ContinueWarning(
                            Strings.NeedRestartRhinoGrasshopperApplyImportedSettings));
                    switch (transferResult)
                    {
                        case TransferResult.Succeeded:
                            UiHelper.Information(Strings.SettingsSuccesfullyApplied);
                            break;
                        case TransferResult.Failed:
                            UiHelper.Information(Strings.CannotApplyCertainSettingsRestartRhinoAgainRNIfProblemPersistsContact);
                            break;
                        case TransferResult.RestartRequired:
                            UiHelper.Information(Strings.SettingsSuccesfullyAppliedShouldRestartRhinoThatChangesSaved);
                            break;
                        case TransferResult.Cancelled:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal enum TransferResult
    {
        Succeeded,
        Failed,
        RestartRequired,
        Cancelled
    }

    internal static TransferResult SaveToFile(string filepath, IEnumerable<Modules.TransferSetting.Base> providers)
    {
        if (File.Exists(filepath))
            File.Delete(filepath);

        //using (var zip = ZipFile.Open(filepath, ZipArchiveMode.Create))
        //{
        //    zip.CreateEntry("gh_version").FromBytes(Encoding.ASCII.GetBytes(Versioning.Version.ToString()));
        //    zip.CreateEntry("pancake_version").FromBytes(Encoding.ASCII.GetBytes(Updater.CoreVersion.ToString(4)));
        //    zip.CreateEntry("create_time")
        //        .FromBytes(Encoding.ASCII.GetBytes(DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture)));

        //    foreach (var provider in providers)
        //    {
        //        var data = provider.SaveToByteArary();
        //        zip.CreateEntry(provider.Guid.ToString("D").ToLowerInvariant())
        //            .FromBytes(data);
        //        data = null;
        //    }
        //}

        return TransferResult.Succeeded;
    }

    internal static TransferResult LoadFromFile(string filepath, IEnumerable<Base> providers, Func<bool> restartCheck = null)
    {
        var list = providers.ToList();

        var restart = list.Any(x => x.RestartRequired);
        if (restart && !(restartCheck?.Invoke() ?? true))
            return TransferResult.Cancelled;

        //using (var zip = ZipFile.Open(filepath, ZipArchiveMode.Read))
        //{
        //    var rels = new Dictionary<string, Modules.TransferSetting.Base>();
        //    foreach (var provider in list)
        //        rels.Add(provider.Guid.ToString("D").ToLowerInvariant(), provider);

        //    foreach (var entry in zip.Entries)
        //        if (rels.ContainsKey(entry.Name))
        //        {
        //            using (var stream = entry.Open())
        //            using (var mem = new MemoryStream())
        //            {
        //                stream.CopyTo(mem);
        //                rels[entry.Name].LoadFromByteArray(mem.ToArray());
        //            }
        //        }
        //}

        return restart ? TransferResult.RestartRequired : TransferResult.Succeeded;
    }

    internal static string GetSettingXmlName(string key)
    {
        throw new NotImplementedException();
        // return Path.Combine(Folders.SettingsFolder, $"{key}.xml");
    }

    internal static string GetSettingName(string key)
    {
        throw new NotImplementedException();
        // return Path.Combine(Folders.SettingsFolder, key);
    }
}
