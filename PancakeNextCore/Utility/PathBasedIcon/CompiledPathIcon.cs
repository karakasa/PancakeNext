using Eto.Drawing;
using Grasshopper2.UI.Icon;
using SvgIcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility.PathBasedIcon;
internal sealed class CompiledPathIcon : AbstractIcon
{
    static readonly List<CompiledPathIcon> IconInstances = [];
    private enum DarkModeSpecialValue
    {
        None,
        Black,
        White
    }
    private readonly struct FillElement(DarkModeSpecialValue darkMode, Color color, Brush artist, IGraphicsPath path)
    {
        public readonly DarkModeSpecialValue DarkModeSpecial = darkMode;
        public readonly Color OriginalColor = color;
        public readonly Brush Artist = artist;
        public readonly IGraphicsPath Path = path;
    }
    private readonly struct PathElement(DarkModeSpecialValue darkMode, Color color, float width, Pen artist, IGraphicsPath path)
    {
        public readonly DarkModeSpecialValue DarkModeSpecial = darkMode;
        public readonly Color OriginalColor = color;
        public readonly float StrokeWidth = width;
        public readonly Pen Artist = artist;
        public readonly IGraphicsPath Path = path;
    }

    static readonly Dictionary<Color, Brush> _brushes = [];
    static readonly Dictionary<StrokeDescription, Pen> _pen = [];

    FillElement[] _fillCache = [];
    PathElement[] _pathCache = [];
    public CompiledPathIcon(PathIcon icon) : base(IconType.Vector)
    {
        IconInstances.Add(this);

        List<FillElement> fillCache = [];
        List<PathElement> pathCache = [];

        foreach (var elem in icon.Regions.Where(x => x.Type == RegionElementType.Fill))
        {
            var dm = GetDarkModeSpecial(elem.FillColor);
            var brush = GetBrush(dm, elem.FillColor);
            fillCache.Add(new(dm, elem.FillColor, brush, elem.ToGraphicsPath()));
        }

        foreach (var grp in icon.Regions.Where(x => x.Type is RegionElementType.OpenPath or RegionElementType.Path).GroupBy(x => x.Stroke))
        {
            foreach (var item in grp)
            {
                var dm = GetDarkModeSpecial(item.StrokeColor);
                var pen = GetPen(dm, item.Stroke);
                pathCache.Add(new(dm, item.StrokeColor, item.StrokeWidth, pen, item.ToGraphicsPath()));
            }
        }

        _fillCache = fillCache.ToArray();
        _pathCache = pathCache.ToArray();
    }
    private static DarkModeSpecialValue GetDarkModeSpecial(Color color)
    {
        if (color.Equals(Colors.Black)) return DarkModeSpecialValue.Black;
        if (color.Equals(Colors.White)) return DarkModeSpecialValue.White;
        return DarkModeSpecialValue.None;
    }
    private static Brush GetBrush(DarkModeSpecialValue dm, Color color)
    {
        if (IconHost.DarkMode)
        {
            color = dm switch
            {
                DarkModeSpecialValue.Black => Colors.White,
                DarkModeSpecialValue.White => Colors.Black,
                _ => color
            };
        }

        if (_brushes.TryGetValue(color, out var brush)) return brush;
        return _brushes[color] = new SolidBrush(color);
    }
    private static Pen GetPen(DarkModeSpecialValue dm, StrokeDescription desc)
    {
        if (IconHost.DarkMode)
        {
            desc = dm switch
            {
                DarkModeSpecialValue.Black => new(Colors.White, desc.Width),
                DarkModeSpecialValue.White => new(Colors.Black, desc.Width),
                _ => desc
            };
        }

        if (_pen.TryGetValue(desc, out var pen)) return pen;
        return _pen[desc] = new Pen(desc.Color, desc.Width);
    }
    private static bool IsIdentityScale(float proposedWidth, float proposedHeight, float size)
    {
        const float tol = 0.01f;
        return proposedWidth > size - tol && proposedWidth < size + tol && proposedHeight > size - tol && proposedHeight < size + tol;
    }
    protected override sealed void DrawInternal(IconContext context)
    {
        var frame = context.Frame;
        var g = context.Context.Graphics;

        float iconSize = 24.0f;
        using var iconXform = g.SaveTransformState();

        if (!IsIdentityScale(frame.Width, frame.Height, iconSize))
            g.ScaleTransform(frame.Width / iconSize, frame.Height / iconSize);
        g.TranslateTransform(frame.X, frame.Y);

        foreach (var kv in _fillCache)
        {
            var brush = kv.Artist;
            var gpath = kv.Path;
            g.FillPath(brush, gpath);
        }

        foreach (var kv in _pathCache)
        {
            var brush = kv.Artist;
            var gpath = kv.Path;
            g.DrawPath(brush, gpath);
        }
    }

    private void HandleDarkMode()
    {
        List<FillElement> fillCache = new(_fillCache.Length);
        List<PathElement> pathCache = new(_pathCache.Length);

        foreach (var elem in _fillCache)
        {
            var brush = GetBrush(elem.DarkModeSpecial, elem.OriginalColor);
            fillCache.Add(new(elem.DarkModeSpecial, elem.OriginalColor, brush, elem.Path));
        }

        foreach (var elem in _pathCache)
        {
            var pen = GetPen(elem.DarkModeSpecial, new(elem.OriginalColor, elem.StrokeWidth));
            pathCache.Add(new(elem.DarkModeSpecial, elem.OriginalColor, elem.StrokeWidth, pen, elem.Path));
        }

        _fillCache = fillCache.ToArray();
        _pathCache = pathCache.ToArray();
    }
    public static void RefreshAllForDarkModeChange()
    {
        foreach (var icon in IconInstances)
            icon.HandleDarkMode();
    }
}
