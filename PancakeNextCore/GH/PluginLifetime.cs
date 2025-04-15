using Grasshopper2.UI;
using PancakeNextCore.Dataset;
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

    static bool _iconLoaded = false;
    public static void HandleIconEvent()
    {
        if (_iconLoaded) return;

        PostUiLoad();

        _iconLoaded = true;
    }

    static void RegisterCloseEvent()
    {
        Editor.Instance.Closing += static (o, e) => BeforeUiClose();
    }

    public static void PreUiLoad()
    {
        PreUiLoaded?.Invoke(null, EventArgs.Empty);

        KnockKnockAreYouAwake.WaitAtDoor();
    }

    public static void PostUiLoad()
    {
        RegisterCloseEvent();
        CoreMenu.Instance.RegisterMenu();
        PostUiLoaded?.Invoke(null, EventArgs.Empty);
    }
    public static void BeforeUiClose()
    {
        UiClosing?.Invoke(null, EventArgs.Empty);
        Config.SaveToFile();
    }
}
