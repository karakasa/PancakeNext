using Eto.Drawing;
using Grasshopper2;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Framework;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI.Canvas;
using PancakeNextCore.GH.Params;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PancakeNextCore.GH.Tweaks;

public sealed class GhParamContentArtist : ICanvasArtist
{
    private static bool DrawAbove { get; set; } = false;
    private static bool DrawObjectLabels { get; set; }

    private static Font _font;
    private static Brush _brushNormal;
    private static Brush _brushGreyedOut;

    private static bool GrayedOut = false;
    private static void Paint(object? sender, CanvasPaintEventArgs ev)
    {
        var canvas = ev.Canvas;
        var zoom = canvas.Projection.Zoom;
        
        if (zoom < 1)
            return;

        GrayedOut = zoom < 1.5;

        ProcessParameters(ev);
    }

    private static void InitializeStyles()
    {
        _font = new Font(FontFamilies.Sans, 8);
        _brushNormal = new SolidBrush(Colors.Black);
        _brushGreyedOut = new SolidBrush(Colors.DarkGray);
    }

    private static void DisposeStyles()
    {
        _font?.Dispose();
        _font = null;
        _brushNormal?.Dispose();
        _brushNormal = null;
        _brushGreyedOut?.Dispose();
        _brushGreyedOut = null;
    }

    private static bool TryGetFirstData<T>(Parameter<T> param, [NotNullWhen(true)] out T? item)
    {
        if (param.State?.Data?.Tree() is not Grasshopper2.Data.Tree<T> tree || tree.LeafCount == 0)
        {
            item = default;
            return false;
        }

        item = tree.Items[0, 0]!;
        return item is not null;
    }

    private static string? DescribeFirstData<T>(Parameter<T> param)
    {
        if (TryGetFirstData(param, out var item))
            return item.ToString();

        return null;
    }

    private static string? DescribeFirstData(IParameter param)
    {
        return param switch
        {
            IntegerParameter pInt => DescribeFirstData(pInt),
            NumberParameter pNum => DescribeFirstData(pNum),
            TextParameter pText => DescribeFirstData(pText),
            BooleanParameter pBoolean => DescribeFirstData(pBoolean),
            QuantityParameter pQuantity => DescribeFirstData(pQuantity),
            _ => null,
        };
    }

    private static bool IsSimpleParameter(IParameter param)
    {
        return param is IntegerParameter or NumberParameter or TextParameter or BooleanParameter or QuantityParameter;
    }

    private static void ProcessParameters(CanvasPaintEventArgs ev)
    {
        if(ev.Canvas is not {} canvas) return;
        var doc = canvas?.Document;
        if (doc == null) return;
        var activeRegion = canvas.VisibleFrame;

        foreach (var it in doc.Objects.ActiveObjects.OfType<IParameter>())
        {
            if (it.Kind is not Kind.Floating) continue;

            var bbox = it.Attributes.Bounds;
            if (!activeRegion.Intersects(bbox))
                continue;

            if (IsSimpleParameter(it))
            {
                DrawParameterContentPreview(it, ev);
            }
        }
    }
    private static void DrawParameterContentPreview(IParameter it, CanvasPaintEventArgs ev)
    {
        var state = it.State;
        if (state is null || state.Phase != Phase.Completed) return;

        var tree = state.Data?.Tree();
        if (tree is null) return;

        var dataCnt = tree.LeafCount;
        var str = DescribeFirstData(it);
        if (str is null) return;

        var bbox = it.Attributes.Bounds;

        if (dataCnt > 1)
        {
            if (str.Length >= 22)
                str = str.Substring(0, 22);
            str = $"{str}{Environment.NewLine}...+ {dataCnt - 1}";
        }
        else if (str.Length > 33)
            str = str.Substring(0, 33);

        var height = bbox.Height;
        var y = bbox.Y;

        if (DrawAbove || !DrawObjectLabels)
        {
            y += height + 6.0f; // Draw below component
        }
        else
        {
            y -= height + 6.0f; // Draw above component
        }

        var rect = new RectangleF(bbox.X, y, bbox.Width, height);

        var graphics = ev.Graphics.Content;
        graphics.DrawTextInFrame(_font, ActiveBrush, str, rect, Eto.Forms.TextAlignment.Center);
    }

    public void Register(Canvas canvas)
    {
        DrawAboveSettingChanged();
        DrawLabelsSettingChanged();
        Unregister(canvas);

        InitializeStyles();
        Settings.CanvasLabelAbove.Changed += DrawAboveSettingChanged;
        Settings.CanvasDrawLabels.Changed += DrawLabelsSettingChanged;
        canvas.AfterPaintObjects += Paint;
    }

    private void DrawLabelsSettingChanged(object? sender = null, EventArgs? e = null)
    {
        DrawObjectLabels = Settings.CanvasDrawLabels;
    }

    private static void DrawAboveSettingChanged(object? sender = null, EventArgs? e = null)
    {
        DrawAbove = Settings.CanvasLabelAbove;
    }

    public void Unregister(Canvas canvas)
    {
        canvas.AfterPaintObjects -= Paint;
        Settings.CanvasLabelAbove.Changed -= DrawAboveSettingChanged;
        Settings.CanvasDrawLabels.Changed -= DrawLabelsSettingChanged;
        DisposeStyles();
    }

    private static Brush ActiveBrush => GrayedOut ? _brushGreyedOut : _brushNormal;

    public const string ConstName = "ParamContentArtist";
    public string Name => ConstName;
}
