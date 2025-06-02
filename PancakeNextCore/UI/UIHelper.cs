using Eto.Forms;
using Grasshopper2.Diagnostics;
using PancakeNextCore.Dataset;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PancakeNextCore.UI;

internal static partial class UiHelper
{
    internal static bool OpenFolder(string folder)
    {
        if (!Directory.Exists(folder))
            return false;

        try
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = folder,
                UseShellExecute = true,
                Verb = "open"
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    internal static bool OpenFileSelected(string path)
    {
        try
        {
            if (!File.Exists(path)) return false;
            var fullpath = Path.GetFullPath(path);

            if (Config.IsRunningOnMac)
            {
                Process.Start("open", $"-a Finder \"{fullpath}\"");
            }
            else
            {
                Process.Start("explorer.exe", $"/select,\"{fullpath}\"");
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static readonly string Title = Strings.UiHelper_PancakeTitle;
    private static readonly string TitleErr = Strings.UiHelper_PancakeTitleError;

    internal static void InvokeUi(Action action, bool blocking = false)
    {
        if (blocking)
        {
            InvokeUi(() => { action(); return 0; });
        }
        else
        {
            Application.Instance?.AsyncInvoke(action);
        }
    }
    internal static T InvokeUi<T>(Func<T> action)
    {
        var inst = Application.Instance;
        if (inst is null) return default!;

        try
        {
            inst.EnsureUIThread();
            return action();
        }
        catch
        {
            return inst.InvokeAsync(action).Result;
        }
    }
    internal static void MinorError(string errCode, string desc, bool blocking = false)
    {
        Logger.Add(Level.Warning, "[Pancake] " + errCode + " " + desc ?? string.Empty);
        InvokeUi(() => MessageBox.Show(desc, Title, MessageBoxButtons.OK), blocking);
    }

    internal static void ErrorReport(string errCode, string desc = "", bool blocking = false)
    {
        if (string.IsNullOrEmpty(desc))
            desc = Strings.UiHelper_ErrorReport_Default;
        Logger.Add(Level.Error, "[Pancake] " + errCode + " " + desc);
        InvokeUi(() => MessageBox.Show(string.Format(Strings.UiHelper_ErrorReport_Template, desc, errCode), TitleErr), blocking);
    }

    internal static bool UndoWarning(string optional = "")
    {
        return ContinueWarning(string.Format(Strings.UiHelper_UndoWarning_Template, optional));
    }

    internal static bool ContinueWarning(string optional = "")
    {
        var notifytext = Strings.UiHelper_ContinueWarning_Template;
        notifytext = string.Format(notifytext, optional);
        return InvokeUi(() => MessageBox.Show(notifytext, Title, MessageBoxButtons.OKCancel)) == DialogResult.Ok;
    }

    public static void Information(string info = "", bool blocking = false)
    {
        InvokeUi(() => MessageBox.Show(info, Title, MessageBoxButtons.OK), blocking);
    }

    public static void UnavailableInReleaseVersion()
    {
#if DEBUG
        Information("[TRAP] This shouldn't be called. Check code.");
#else
        Information("This feature is unavailable in non-nightly builds of Pancake.");
#endif
    }

    public static void InformationRequireRestart()
    {
        Information(Strings.CoreMenu_mnuAlwaysEnglish_Click_TheSettingWillTakeEffectAfterYouRestartRhino);
    }

    public static bool AskYesNo(string info)
    {
        return InvokeUi(() => MessageBox.Show(info, Title, MessageBoxButtons.YesNo)) == DialogResult.Yes;
    }

    //public static string SaveFileDialog(string title, string filter)
    //{
    //    using (var uiSave = new SaveFileDialog())
    //    {
    //        uiSave.AddExtension = true;
    //        uiSave.OverwritePrompt = true;
    //        uiSave.Title = title;
    //        uiSave.Filter = filter;
    //        uiSave.InitialDirectory = ProjectHelper.DefaultProjectDirectory;

    //        return uiSave.ShowDialog() == DialogResult.OK ? uiSave.FileName : "";
    //    }
    //}

    //public static string OpenFileDialog(string title, string filter)
    //{
    //    using (var uiSave = new OpenFileDialog())
    //    {
    //        uiSave.AddExtension = true;
    //        uiSave.Title = title;
    //        uiSave.Filter = filter;
    //        uiSave.InitialDirectory = ProjectHelper.DefaultProjectDirectory;

    //        return uiSave.ShowDialog() == DialogResult.OK ? uiSave.FileName : "";
    //    }
    //}

    //public static string FolderBrowserDialog(string title)
    //{
    //    using (var uiFolder = new FolderBrowserDialog())
    //    {
    //        uiFolder.Description = title;
    //        uiFolder.ShowNewFolderButton = true;

    //        return uiFolder.ShowDialog() == DialogResult.OK ? uiFolder.SelectedPath : "";
    //    }
    //}

    public static void OpenUrl(string url)
    {
        try
        {
            // This should also works on macOS.
            Process.Start(url);
        }
        catch
        {
        }
    }

    internal static string? OpenFileDialog(string openPackedSettings, string extensionHint)
    {
        return "";
        throw new NotImplementedException();
    }

    internal static string? SaveFileDialog(string savePackedSettings, string extensionHint)
    {
        return "";
        throw new NotImplementedException();
    }
}
