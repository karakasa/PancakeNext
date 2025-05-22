using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvgIcon;
public readonly struct StrokeDescription(Color color, float width) : IEquatable<StrokeDescription>
{
    public Color Color { get; } = color;
    public float Width { get; } = width;

    public bool Equals(StrokeDescription other)
    {
        return Width == other.Width && Color == other.Color;
    }
    public override bool Equals(object obj)
    {
        return obj is StrokeDescription desc && Equals(desc);
    }
    public override int GetHashCode()
    {
        return unchecked(Color.ToArgb() * -1777771 + Width.GetHashCode());
    }
    public override string ToString()
    {
        return $"{Color}, w{Width}";
    }
    public static bool operator ==(StrokeDescription left, StrokeDescription right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(StrokeDescription left, StrokeDescription right)
    {
        return !(left == right);
    }
}
