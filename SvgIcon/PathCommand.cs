using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvgIcon;
public abstract class PathCommand(PathCommandType type)
{
    public PathCommandType Type { get; } = type;

    public virtual void ReadFrom(BinaryReader reader) { }
#if DEBUG
    public virtual void WriteTo(BinaryWriter writer)
    {
        writer.Write((byte)Type);
    }
#endif
    public abstract void AppendToGraphicsPath(IGraphicsPath path);
    public static PathCommand ReadNext(BinaryReader reader)
    {
        var type = (PathCommandType)reader.ReadByte();
        PathCommand next = type switch
        {
            PathCommandType.EOF => EofCommand.Instance,
            PathCommandType.Line => new LineCommand(),
            PathCommandType.Polyline => new PolylineCommand(),
            PathCommandType.Arc => new ArcCommand(),
            PathCommandType.Ellipse => new EllipseCommand(),
            _ => throw new NotSupportedException(),
        };
        next.ReadFrom(reader);
        return next;
    }
}
public sealed class LineCommand() : PathCommand(PathCommandType.Line)
{
    public float startX;
    public float startY;
    public float endX;
    public float endY;

    public override void AppendToGraphicsPath(IGraphicsPath path)
    {
        path.AddLine(startX, startY, endX, endY);
    }

    public override void ReadFrom(BinaryReader reader)
    {
        base.ReadFrom(reader);

        startX = reader.ReadSingle();
        startY = reader.ReadSingle();
        endX = reader.ReadSingle();
        endY = reader.ReadSingle();
    }

#if DEBUG
    public override void WriteTo(BinaryWriter writer)
    {
        base.WriteTo(writer);

        writer.Write(startX);
        writer.Write(startY);
        writer.Write(endX);
        writer.Write(endY);
    }
#endif
}
public sealed class PolylineCommand() : PathCommand(PathCommandType.Polyline)
{
    public List<PointF>? points;

    public override void AppendToGraphicsPath(IGraphicsPath path)
    {
        if (points is null) return;
        path.AddLines(points);
    }

    public override void ReadFrom(BinaryReader reader)
    {
        base.ReadFrom(reader);

        var n = reader.ReadInt32();
        points = new List<PointF>(n);

        for (var i = 0; i < n; i++)
        {
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();

            points.Add(new PointF(x, y));
        }
    }
#if DEBUG
    public override void WriteTo(BinaryWriter writer)
    {
        base.WriteTo(writer);

        writer.Write(points.Count);
        foreach (var pt in points)
        {
            writer.Write(pt.X);
            writer.Write(pt.Y);
        }
    }
#endif
}

public sealed class EllipseCommand() : PathCommand(PathCommandType.Ellipse)
{
    public float x;
    public float y;
    public float width;
    public float height;

    public override void AppendToGraphicsPath(IGraphicsPath path)
    {
        path.AddEllipse(x, y, width, height);
    }

    public override void ReadFrom(BinaryReader reader)
    {
        base.ReadFrom(reader);

        x = reader.ReadSingle();
        y = reader.ReadSingle();
        width = reader.ReadSingle();
        height = reader.ReadSingle();
    }
#if DEBUG
    public override void WriteTo(BinaryWriter writer)
    {
        base.WriteTo(writer);

        writer.Write(x);
        writer.Write(y);
        writer.Write(width);
        writer.Write(height);
    }
#endif
}
public sealed class ArcCommand() : PathCommand(PathCommandType.Arc)
{
    public float x;
    public float y;
    public float width;
    public float height;
    public float startAngle;
    public float sweepAngle;

    public override void AppendToGraphicsPath(IGraphicsPath path)
    {
        path.AddArc(x, y, width, height, startAngle, sweepAngle);
    }

    public override void ReadFrom(BinaryReader reader)
    {
        base.ReadFrom(reader);

        x = reader.ReadSingle();
        y = reader.ReadSingle();
        width = reader.ReadSingle();
        height = reader.ReadSingle();
        startAngle = reader.ReadSingle();
        sweepAngle = reader.ReadSingle();
    }
#if DEBUG
    public override void WriteTo(BinaryWriter writer)
    {
        base.WriteTo(writer);

        writer.Write(x);
        writer.Write(y);
        writer.Write(width);
        writer.Write(height);
        writer.Write(startAngle);
        writer.Write(sweepAngle);
    }
#endif
}
public sealed class EofCommand() : PathCommand(PathCommandType.EOF)
{
    public static readonly EofCommand Instance = new();
    public override void AppendToGraphicsPath(IGraphicsPath path)
    {
        throw new NotSupportedException();
    }
}