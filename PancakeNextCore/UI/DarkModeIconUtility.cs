using Pancake.Dataset;
using Pancake.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.UI;

internal static class DarkModeIconUtility
{
    private static Dictionary<string, Bitmap> _cache;
    public static Bitmap GetDarkModeIcon(string name, Bitmap lightMode)
    {
        try
        {
            EnsureCache();

            if (_cache.TryGetValue(name, out var bmp))
                return bmp;

            if (!Config.IsDebugVersion)
                LogUtility.Warning($"Dark icon for {name} not found. It shouldn't happen.");

            bmp = Convert(lightMode);
            if (bmp is null)
                return lightMode;

            return _cache[name] = bmp;
        }
        catch
        {
            // Crashing here would result in a weird GH interface.
            // Fall back in such case.
            return lightMode;
        }
    }

    private static void EnsureCache()
    {
        if (_cache is not null) return;
        _cache = new();
        ReadFromCache(DarkComponentIcons.DarkIconCache);
    }
    private static Bitmap Convert(Bitmap bmp)
    {
        var bmp2 = new Bitmap(bmp);

        for (var i = 0; i < bmp2.Width; i++)
            for (var j = 0; j < bmp2.Height; j++)
            {
                var color = bmp2.GetPixel(i, j);

                if (color.R == 0 && color.G == 0 && color.B == 0)
                    bmp2.SetPixel(i, j, Color.FromArgb(color.A, 255, 255, 255));
            }

        return bmp2;
    }

    public static void DumpAll()
    {
#if DEBUG
        const string FilePath = @"D:\temp\PancakeDarkIcons.bin";
        using var fs = new FileStream(FilePath, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        using var ms = new MemoryStream(1024 * 1024);

        writer.Write(_cache.Count);

        foreach (var it in _cache)
        {
            ms.SetLength(0);
            ms.Seek(0, SeekOrigin.Begin);

            writer.Write(it.Key);
            it.Value.Save(ms, ImageFormat.Png);
            var img = ms.ToArray();
            writer.Write(img.Length);
            writer.Write(img);
        }
#endif
    }

    public static void ReadFromCache(byte[] cache)
    {
        using var ms = new MemoryStream(cache, false);
        using var reader = new BinaryReader(ms);

        var cnt = reader.ReadInt32();
        for (var i = 0; i < cnt; i++)
        {
            var key = reader.ReadString();
            var imgLength = reader.ReadInt32();

            var imgBytes = reader.ReadBytes(imgLength);

            using var ms2 = new MemoryStream(imgBytes, false);

            if (Image.FromStream(ms2) is not Bitmap img)
                continue;

            _cache[key] = img;
        }
    }
}
