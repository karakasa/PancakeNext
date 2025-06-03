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
    public List<ColorDescriptor> Colors { get; } = [];
    public string? SvgFallback { get; set; }

    [Flags]
    internal enum StorageFlag : int
    {
        HasSvgFallback = 1,
        HasColorDescriptor = 2
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
        var flags = GetFlags();

        writer.Write(MagicNumber);
        writer.Write((int)flags);

        if (SvgFallback != null)
        {
            writer.Write(SvgFallback);
        }

        if ((flags & StorageFlag.HasColorDescriptor) != 0)
        {
            writer.Write(Colors.Count);
            foreach (var color in Colors)
            {
                color.WriteTo(writer);
            }
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

    internal StorageFlag FlagsDuringRead { get; private set; }
    internal bool HasFlag(StorageFlag flag) => (FlagsDuringRead & flag) != 0;
    private void ReadFrom(BinaryReader reader)
    {
        FlagsDuringRead = 0;

        var magicNumber = reader.ReadInt32();
        if (magicNumber != MagicNumber)
        {
            throw new InvalidDataException("Invalid file header.");
        }

        FlagsDuringRead = (StorageFlag)reader.ReadInt32();

        if (HasFlag(StorageFlag.HasSvgFallback))
        {
            SvgFallback = reader.ReadString();
        }

        int n;

        if (HasFlag(StorageFlag.HasColorDescriptor))
        {
            Colors.Clear();

            n = reader.ReadInt32();
            for (var i = 0; i < n; i++)
            {
                Colors.Add(ColorDescriptor.CreateFrom(reader));
            }
        }

        Regions.Clear();

        n = reader.ReadInt32();
        for (var i = 0; i < n; i++)
        {
            var loop = new RegionElement();
            loop.ReadFrom(reader, this);
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
