using Eto.Forms;
using PancakeNextCore.Modules.TransferSetting;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PancakeNextCore.UI;

internal class Presenter
{
    public static void ShowReportWindow(string report, bool wrap = false)
    {
        var shownForm = new EtoForms.ReportUi();
        shownForm.SetContent(report, wrap);
        shownForm.Owner = EtoExtensions.GetGrasshopperWindowAsEto();
        shownForm.Show();
    }

    public static void ShowWindow<T>() where T : Form, new()
    {
        var shownForm = new T();
        shownForm.Owner = EtoExtensions.GetGrasshopperWindowAsEto();
        shownForm.Show();
    }

    internal static TransferWindowResult ShowTransferWindow(IReadOnlyList<Base> instances, out List<Base> selected)
    {
        selected = [];
        return TransferWindowResult.Cancelled;
        throw new NotImplementedException();
    }

    /*public static void ShowPluginManagerWindow(IList<string> names, IList<string> searchKey,
        IList<Eto.Drawing.Image> pluginIcons,
        IList<List<KeyValuePair<string, string>>> pluginInfo, IList<List<KeyValuePair<string, string>>> pluginFiles,
        IList<List<KeyValuePair<string, string>>> pluginObjs,
        IList<string> append1, IList<string> append2,
        Action<EtoForms.FormAddonManager, string, int, string, string, int> procedure = null)
    {
        PersistentEtoForm<EtoForms.FormAddonManager>.Show(null, form =>
        {
            // form.procedure = procedure;

            for (var i = 0; i < names.Count; i++)
            {
                form.AddPlugin(searchKey[i], names[i], pluginIcons[i], append1[i], append2[i], i);
                form.AddPluginInfo(names[i], pluginInfo[i], pluginFiles[i], pluginObjs[i], i);
            }
        });
    }
    internal static void ShowPortabilityReport()
    {
        //if (Config.IsMac)
        //{
        //    var report = PortabilityReport.GenerateDefaultPortabilityReport();
        //    ShowReportWindow(report);
        //    return;
        //}

        var doc = Instances.ActiveCanvas?.Document;
        if (doc == null)
            return;

        PersistentEtoForm<EtoForms.FormPortabilityReport>.Show();

        // PersistentForm<FormPortabilityReport>.Show();
    }*/
    
    public enum TransferWindowResult
    {
        Cancelled,
        SaveToFile,
        LoadFromFile
    }
}
