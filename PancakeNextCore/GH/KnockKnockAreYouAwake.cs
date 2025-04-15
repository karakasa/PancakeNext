using Grasshopper2.UI;
using PancakeNextCore.Dataset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH;

/// <summary>
/// Unlike GH1 (Instances.CanvasCreated), there's no post-UI-creation event in GH2. We need to do a inquiry loop.
/// </summary>
internal static class KnockKnockAreYouAwake
{
    public static void WaitAtDoor()
    {
        new Thread(KnockKnock).Start();
    }

    private static async void KnockKnock()
    {
        try
        {
            var etoApp = Eto.Forms.Application.Instance;
            if (etoApp is null) return; // This shouldn't happen, though.

            const int TimeoutPerWait = 500; // 0.5s
            const int MaxWaitCount = 10 * 60 * 1000 / TimeoutPerWait; // 10min

            for (var i = 0; ; i++)
            {
                if (i > MaxWaitCount)
                    break;

                var vi = await etoApp.InvokeAsync(() => Editor.Instance);
                if (vi is null)
                {
                    Thread.Sleep(TimeoutPerWait);
                    continue;
                }

                etoApp.AsyncInvoke(PluginLifetime.PostUiLoad);
                break;
            }
        }
        catch (Exception ex)
        {
            IssueTracker.ReportInPlace(ex.Message);
            // We cannot crash here because we are running in a separate thread, which would crash Rhino.
        }
    }
}
