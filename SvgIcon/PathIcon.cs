using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SvgIcon;

public sealed class PathIcon
{
    const int MagicNumber = 1665753936;
    public List<RegionElement> Regions { get; } = [];
    public string? SvgFallback { get; set; }

    [Flags]
    private enum StorageFlag : int
    {
        HasSvgFallback = 1
    }
    private StorageFlag GetFlags()
    {
        StorageFlag flag = 0;
        if (SvgFallback != null)
        {
            flag |= StorageFlag.HasSvgFallback;
        }

        return flag;
    }
#if DEBUG
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(MagicNumber);
        writer.Write((int)GetFlags());

        if (SvgFallback != null)
        {
            writer.Write(SvgFallback);
        }

        writer.Write(Regions.Count);
        foreach (var loop in Regions)
        {
            loop.WriteTo(writer);
        }
    }
    
    public void WriteTo(string path)
    {
        using var fs = File.Open(path, FileMode.Create);
        using var writer = new BinaryWriter(fs, Encoding.UTF8);

        WriteTo(writer);
    }
#endif
    private void ReadFrom(BinaryReader reader)
    {
        var magicNumber = reader.ReadInt32();
        if (magicNumber != MagicNumber)
        {
            throw new InvalidDataException("Invalid file header.");
        }

        var flags = (StorageFlag)reader.ReadInt32();

        if ((flags & StorageFlag.HasSvgFallback) != 0)
        {
            SvgFallback = reader.ReadString();
        }

        Regions.Clear();

        var n = reader.ReadInt32();
        for (var i = 0; i < n; i++)
        {
            var loop = new RegionElement();
            loop.ReadFrom(reader);
            Regions.Add(loop);
        }
    }

    public static PathIcon CreateFrom(BinaryReader reader)
    {
        var icon = new PathIcon();
        icon.ReadFrom(reader);
        return icon;
    }

}
