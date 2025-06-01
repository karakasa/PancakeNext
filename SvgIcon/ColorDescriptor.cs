using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SvgIcon;
public enum ColorDescriptorType : byte
{
    Undefined,
    Solid
}
public abstract class ColorDescriptor
{
    public abstract ColorDescriptorType Type { get; }
#if DEBUG
    public virtual void WriteTo(BinaryWriter writer) { writer.Write((byte)Type); }
#endif

    public virtual void ReadFrom(BinaryReader reader) { }
    public static ColorDescriptor CreateFrom(BinaryReader reader)
    {
        var type = (ColorDescriptorType)reader.ReadByte();
        ColorDescriptor desc = type switch
        {
            ColorDescriptorType.Solid => new SolidColorDescriptor(),
            _ => throw new InvalidDataException($"Unknown color descriptor type: {type}"),
        };

        desc.ReadFrom(reader);
        return desc;
    }
}
public sealed class SolidColorDescriptor : ColorDescriptor
{
    public override ColorDescriptorType Type => ColorDescriptorType.Solid;
    public Color Color { get; set; }
#if DEBUG
    public override void WriteTo(BinaryWriter writer)
    {
        base.WriteTo(writer);
        writer.Write(Color.ToArgb());
    }
#endif

    public override void ReadFrom(BinaryReader reader)
    {
        Color = Color.FromArgb(reader.ReadInt32());
    }
}