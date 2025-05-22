using Eto.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvgIcon;
public sealed class RegionElement
{
    public RegionElementType Type { get; set; } = RegionElementType.Fill;
    public Color FillColor { get; set; } = Colors.Black;
    public Color StrokeColor { get; set; } = Colors.Black;
    public float StrokeWidth { get; set; } = 0;
    public StrokeDescription Stroke => new(StrokeColor, StrokeWidth);
    public List<PathLoop> Loops { get; } = [];
#if DEBUG
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write((byte)Type);
        writer.Write(FillColor.ToArgb());
        writer.Write(StrokeColor.ToArgb());
        writer.Write(StrokeWidth);
        writer.Write(Loops.Count);

        foreach (var loop in Loops)
        {
            loop.WriteTo(writer);
        }
    }
#endif

    public void ReadFrom(BinaryReader reader)
    {
        Type = (RegionElementType)reader.ReadByte();
        FillColor = Color.FromArgb(reader.ReadInt32());
        StrokeColor = Color.FromArgb(reader.ReadInt32());
        StrokeWidth = reader.ReadSingle();

        Loops.Clear();
        var n = reader.ReadInt32();
        for (var i = 0; i < n; i++)
        {
            var loop = new PathLoop();
            loop.ReadFrom(reader);
            Loops.Add(loop);
        }
    }

    public IGraphicsPath ToGraphicsPath()
    {
        var path = GraphicsPath.Create();
        path.FillMode = FillMode.Alternate;

        foreach (var loop in Loops)
            loop.AppendToGraphicsPath(path, Type == RegionElementType.OpenPath);

        return path;
    }
}
