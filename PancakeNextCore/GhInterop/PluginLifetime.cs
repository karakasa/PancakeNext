using Grasshopper2.UI;
using PancakeNextCore.Dataset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GhInterop;
internal static class PluginLifetime
{
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

    public static void PriorityLoad()
    {

    }

    public static void PostUiLoad()
    {
        RegisterCloseEvent();
    }
    public static void BeforeUiClose()
    {
        Config.SaveToFile();
    }
}
