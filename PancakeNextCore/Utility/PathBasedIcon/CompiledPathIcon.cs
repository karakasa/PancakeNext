﻿using Eto.Drawing;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using GrasshopperIO.DataBase;
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
    private readonly struct FillElement(Brush artist, IGraphicsPath path)
    {
        public readonly Brush Artist = artist;
        public readonly IGraphicsPath Path = path;
    }
    private readonly struct PathElement(Pen artist, IGraphicsPath path)
    {
        public readonly Pen Artist = artist;
        public readonly IGraphicsPath Path = path;
    }

    private bool DarkMode { get; set; }

    static readonly Dictionary<Color, Brush> _brushes = [];
    static readonly Dictionary<StrokeDescription, Pen> _pen = [];
#if DEBUG
    private readonly PathIcon _underlyingIcon;
#endif

    FillElement[] _fillCacheNormal = [];
    PathElement[] _pathCacheNormal = [];
    FillElement[] _fillCacheDark = [];
    PathElement[] _pathCacheDark = [];
    public CompiledPathIcon(PathIcon icon, bool initialDarkMode, float size = 24.0f) : base(IconType.Vector)
    {
        IconSize = size;
#if DEBUG
        _underlyingIcon = icon;
#endif
        DarkMode = initialDarkMode;
        BuildCache(icon);

        IconInstances.Add(this);
    }
    private void BuildCache(PathIcon icon)
    {
        List<FillElement> fillCacheNormal = [];
        List<PathElement> pathCacheNormal = [];
        List<FillElement> fillCacheDark = [];
        List<PathElement> pathCacheDark = [];

        foreach (var elem in icon.Regions)
        {
            var path = elem.ToGraphicsPath();

            if (elem.Type is RegionElementType.Fill)
            {
                // fill

                fillCacheNormal.Add(new(GetBrush(false, elem.FillColor), path));
                fillCacheDark.Add(new(GetBrush(true, elem.FillColor), path));
            }
            else
            {
                // path

                pathCacheNormal.Add(new(GetPen(false, elem.Stroke), path));
                pathCacheDark.Add(new(GetPen(true, elem.Stroke), path));
            }
        }

        _fillCacheNormal = fillCacheNormal.ToArray();
        _pathCacheNormal = pathCacheNormal.ToArray();
        _fillCacheDark = fillCacheDark.ToArray();
        _pathCacheDark = pathCacheDark.ToArray();
    }
    private static Color GetDarkModeMappedColor(Color color)
    {
        return unchecked((uint)color.ToArgb()) switch
        {
            0xFF000000 or 0xFF606060 => Colors.White,
            0xFFFFFFFF => Colors.Black,
            0xFF808080 => Colors.LightGrey,
            _ => color,
        };
    }
    private static Brush GetBrush(bool darkMode, Color color)
    {
        if (darkMode) color = GetDarkModeMappedColor(color);

        if (_brushes.TryGetValue(color, out var brush)) return brush;
        return _brushes[color] = new SolidBrush(color);
    }
    private static Pen GetPen(bool darkMode, StrokeDescription desc)
    {
        if (darkMode) desc = desc.With(GetDarkModeMappedColor(desc.Color));

        if (_pen.TryGetValue(desc, out var pen)) return pen;
        return _pen[desc] = new Pen(desc.Color, desc.Width);
    }
    float IconSize { get; } = 24.0f;
    private bool IsIdentityScale(float proposedWidth, float proposedHeight)
    {
        const float tol = 0.01f;
        return proposedWidth > IconSize - tol && proposedWidth < IconSize + tol && proposedHeight > IconSize - tol && proposedHeight < IconSize + tol;
    }
    protected override sealed void DrawInternal(IconContext context)
    {
        var frame = context.Frame;
        var g = context.Context.Graphics;

        using var iconXform = g.SaveTransformState();

        g.TranslateTransform(frame.X, frame.Y);
        if (!IsIdentityScale(frame.Width, frame.Height))
            // Eto should filter out identity xforms (since in most cases icons are 24x24, except in thumbnail mode)
            // We still do it in case platform implementations are different.
            g.ScaleTransform(frame.Width / IconSize, frame.Height / IconSize);

        // Only canvas is rendered in the dark mode.
        // Ribbon tab & tooltips are always light.
        var parentControl = context.Context.Control;
        var darkMode = DarkMode && ShouldTryDarkMode(parentControl);

        foreach (var kv in darkMode ? _fillCacheDark : _fillCacheNormal)
            g.FillPath(kv.Artist, kv.Path);

        foreach (var kv in darkMode ? _pathCacheDark : _pathCacheNormal)
            g.DrawPath(kv.Artist, kv.Path);
    }

    private static bool ShouldTryDarkMode(IFlexControl control)
    {
        return control is Grasshopper2.UI.Canvas.Canvas;
    }

    public static void NotifyForDarkModeChange(bool newValue)
    {
        foreach (var icon in IconInstances)
            icon.DarkMode = newValue;
    }
#if DEBUG
    public static void DestroyAllCaches()
    {
        foreach (var icon in IconInstances)
            icon.BuildCache(icon._underlyingIcon);
    }
#endif
}
