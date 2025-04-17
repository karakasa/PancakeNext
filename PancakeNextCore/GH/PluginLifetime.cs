using Grasshopper2.UI;
using PancakeNextCore.Dataset;
using PancakeNextCore.PancakeMgr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH;
internal static class PluginLifetime
{
    public static event EventHandler? PreUiLoaded;
    public static event EventHandler? PostUiLoaded;
    public static event EventHandler? UiClosing;

    public static readonly FeatureManager Features = new();
    static void RegisterCloseEvent()
    {
        Editor.Instance.Closing += static (o, e) => BeforeUiClose();
    }

    public static void PreUiLoad()
    {
        Features.LoadFeatures();

        PreUiLoaded?.Invoke(null, EventArgs.Empty);

        Features.ProcessStartupHook(FeatureManager.LoadStage.PreUI);

        KnockKnockAreYouAwake.WaitAtDoor();
    }

    public static void PostUiLoad()
    {
        RegisterCloseEvent();
        
        Features.ProcessStartupHook(FeatureManager.LoadStage.UI);
        PostUiLoaded?.Invoke(null, EventArgs.Empty);

        CoreMenu.Instance.RegisterMenu();
    }
    public static void BeforeUiClose()
    {
        UiClosing?.Invoke(null, EventArgs.Empty);
        Config.SaveToFile();
    }
}
