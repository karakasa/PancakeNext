using Eto.Drawing;
using Grasshopper2;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using PancakeNextCore.Dataset;
using PancakeNextCore.UI;
using PancakeNextCore.Utility.PathBasedIcon;
using SvgIcon;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal static class IconHost
{
    public static bool DarkMode { get; set; }

    public const int Size0 = 24;
    public const int Size1 = 48;
    public const int Size2 = 96;
    public const int Size3 = 192;

    // private static readonly int[] DefaultSizes = [Size0, Size1, Size2, Size3];

    private static IconArchive? _archive;
    private static bool _archiveReadFail;
    [MemberNotNullWhen(true, nameof(_archive))]
    private static bool EnsureIconArchiveLoaded()
    {
        if (_archive is not null) return true;
        if (_archiveReadFail) return false;
        using var stream = typeof(IconHost).Assembly.GetManifestResourceStream("PancakeNextCore.Icons.archive");
        if (stream is null)
        {
            UiHelper.ErrorReport("IconHost", "Cannot find icon archive. DLL is corrupt.");
            _archiveReadFail = true; 
            return false;
        }

        try
        {
            _archive = new IconArchive();
            _archive.ReadFrom(DeflateHelper.Decompress(stream));
        }
        catch (Exception ex)
        {
            UiHelper.ErrorReport("IconHost", "Cannot load icon archive: " + ex.Message);
            _archiveReadFail = true;
            return false;
        }

        RegisterDarkModeHandler();

        return true;
    }

    private static void RegisterDarkModeHandler()
    {
        DarkMode = Settings.DarkMode.Value;
        Settings.DarkMode.Changed += OnDarkModeChanged;
    }

    private static void OnDarkModeChanged(object? sender, EventArgs e)
    {
        DarkMode = Settings.DarkMode.Value;
        CompiledPathIcon.NotifyForDarkModeChange(DarkMode);
    }

    //private static Bitmap CreateBitmapFromPathResource(string svg, int defaultWidth, int defaultHeight)
    //{
    //    return Rhino.UI.ImageResources.CreateEtoBitmap(svg, defaultWidth, defaultHeight, true);
    //}

    //private static IIcon CreateFallbackIcon(string svg)
    //{
    //    var bitmaps = new Bitmap[DefaultSizes.Length];
    //    for (var i = 0; i < DefaultSizes.Length; i++)
    //    {
    //        var size = DefaultSizes[i];
    //        bitmaps[i] = CreateBitmapFromPathResource(svg, size, size);
    //    }

    //    // we can do PixelIcon but David wrote bugged code in PixelIcon.ctor(). Write our own instead.
    //    // return new PixelIcon(bitmaps);
    //    return new PixelIconPlayingMahjong(bitmaps);
    //}

    public static IIcon? CreateFromPathResource(string name)
    {
        if (!EnsureIconArchiveLoaded()) return null;
        if (!_archive.TryGet(name, out var icon)) return null;

        return new CompiledPathIcon(icon, IconHost.DarkMode);
        // return CreateFallbackIcon(icon.SvgFallback);
    }
}
