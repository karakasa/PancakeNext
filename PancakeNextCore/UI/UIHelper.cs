using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Pancake.Dataset;
using Pancake.Utility;

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
    private static double? _dpiSystemScale;

    public static double DpiSystemScale => _dpiSystemScale ??= GetSystemScale();
    internal static double GetSystemScale()
    {
        if (Config.IsMac)
        {
            return GetDpiScaleMac();
        }
        else
        {
            return GetDpiScaleWindows();
        }
    }

    private static double GetDpiScaleMac()
    {
        var screen = GetRhinoScreen();
        if (screen is null)
            return 1.0;

        return screen.DPI / DPI_1X;
    }

    private static Eto.Forms.Screen GetRhinoScreen()
    {
        var handle = Rhino.UI.Runtime.PlatformServiceProvider.Service?.MainRhinoWindow;
        try
        {
            return handle.Screen;
        }
        catch
        {
            return Eto.Forms.Screen.PrimaryScreen;
        }
    }

    private const double DPI_1X = 96.0;
    private static double GetDpiScaleWindows()
    {
        using var graphics = Graphics.FromHwnd(nint.Zero);
        return graphics.DpiX / DPI_1X;
    }

    internal static bool OpenFileSelected(string path)
    {
        try
        {
            if (!File.Exists(path)) return false;
            var fullpath = Path.GetFullPath(path);

            if (Config.IsMac)
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

    // The code is temporarily removed due to incompatibility with Eto.

    //[DllImport("user32.dll")]
    //private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

    //internal static void SetCueBanner(IntPtr hWnd, string msg)
    //{
    //    if (Config.IsMac)
    //        return;
    //    SendMessage(hWnd, 0x1501, 1, msg);
    //}

    private static readonly string Title = Strings.UiHelper_PancakeTitle;
    private static readonly string TitleErr = Strings.UiHelper_PancakeTitleError;

    internal static void MinorError(string errCode, string desc)
    {
        LogUtility.Warning(errCode + " " + desc ?? string.Empty);
        MessageBox.Show(desc, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    internal static void ErrorReport(string errCode, string desc = "")
    {
        if (string.IsNullOrEmpty(desc))
            desc = Strings.UiHelper_ErrorReport_Default;
        LogUtility.Error(errCode + " " + desc);
        MessageBox.Show(string.Format(Strings.UiHelper_ErrorReport_Template, desc, errCode), TitleErr);
    }

    internal static bool UndoWarning(string optional = "")
    {
        return ContinueWarning(string.Format(Strings.UiHelper_UndoWarning_Template, optional));
    }

    internal static bool ContinueWarning(string optional = "")
    {
        var notifytext = Strings.UiHelper_ContinueWarning_Template;
        notifytext = string.Format(notifytext, optional);
        return MessageBox.Show(notifytext,
                Title, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK;
    }

    public static void Information(string info = "")
    {
        MessageBox.Show(info, Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        return MessageBox.Show(info, Title, MessageBoxButtons.YesNo) == DialogResult.Yes;
    }

    public static string SaveFileDialog(string title, string filter)
    {
        using (var uiSave = new SaveFileDialog())
        {
            uiSave.AddExtension = true;
            uiSave.OverwritePrompt = true;
            uiSave.Title = title;
            uiSave.Filter = filter;
            uiSave.InitialDirectory = ProjectHelper.DefaultProjectDirectory;

            return uiSave.ShowDialog() == DialogResult.OK ? uiSave.FileName : "";
        }
    }

    public static string OpenFileDialog(string title, string filter)
    {
        using (var uiSave = new OpenFileDialog())
        {
            uiSave.AddExtension = true;
            uiSave.Title = title;
            uiSave.Filter = filter;
            uiSave.InitialDirectory = ProjectHelper.DefaultProjectDirectory;

            return uiSave.ShowDialog() == DialogResult.OK ? uiSave.FileName : "";
        }
    }

    public static string FolderBrowserDialog(string title)
    {
        using (var uiFolder = new FolderBrowserDialog())
        {
            uiFolder.Description = title;
            uiFolder.ShowNewFolderButton = true;

            return uiFolder.ShowDialog() == DialogResult.OK ? uiFolder.SelectedPath : "";
        }
    }

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
}
