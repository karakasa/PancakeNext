using PancakeNextCore.Dataset;
using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper2.UI.Canvas;
using Grasshopper2.Parameters;
using Grasshopper2.Components;

namespace PancakeNextCore.GH.Tweaks;

public sealed class GhParamAccessArtist : ICanvasArtist
{
    private const string ConfigHintOptional = "HintOptionalParameter";
    private static bool _hintOptional = Config.Read(ConfigHintOptional, false, false);
    public static bool HintOptional
    {
        get => _hintOptional;
        set
        {
            _hintOptional = value;
            Config.Write(ConfigHintOptional, value);
        }
    }

    public const string ConstName = "ParamAccessArtist";
    public string Name => ConstName;

    public void Paint(object? sender, CanvasPaintEventArgs ev)
    {
        ProcessComponents(ev);
    }

    private readonly List<RectangleF> boxesListAccess = new List<RectangleF>();
    private readonly List<RectangleF> boxesListAccessOptional = new List<RectangleF>();
    private readonly List<RectangleF> boxesTreeAccess = new List<RectangleF>();
    private readonly List<RectangleF> boxesTreeAccessOptional = new List<RectangleF>();
    private readonly List<RectangleF> boxesItemAccessOptional = new List<RectangleF>();

    private void ClearBoxList()
    {
        boxesListAccess?.Clear();
        boxesTreeAccess?.Clear();
        boxesItemAccessOptional?.Clear();
        boxesListAccessOptional?.Clear();
        boxesTreeAccessOptional?.Clear();
    }

    private static void DrawBoxList(Graphics graphics, List<RectangleF> list, Pen pen)
    {
        foreach (var it in list)
            graphics.DrawRectangle(pen, it);
    }

    private void DrawBoxList(CanvasPaintEventArgs ev)
    {
        var graphics = ev.Graphics.Content;

        DrawBoxList(graphics, boxesListAccess, PenListAccess);
        DrawBoxList(graphics, boxesTreeAccess, PenTreeAccess);
        DrawBoxList(graphics, boxesItemAccessOptional, PenItemAccessOptional);
        DrawBoxList(graphics, boxesListAccessOptional, PenListAccessOptional);
        DrawBoxList(graphics, boxesTreeAccessOptional, PenTreeAccessOptional);
    }

    private void ProcessComponents(CanvasPaintEventArgs ev)
    {
        var canvas = ev.Canvas;
        var zoomLevel = canvas.Projection.Zoom;

        if (zoomLevel <= 1)
            return;

        var ghDoc = canvas.Document;

        ClearBoxList();

        foreach (var component in ghDoc.Objects.ActiveObjects.OfType<Component>())
        {
            ProcessParams(canvas, component.Parameters.Inputs, true);
            ProcessParams(canvas, component.Parameters.Outputs, false);
        }

        DrawBoxList(ev);
        ClearBoxList();
    }

    public Color ColorListAccess = Colors.Red;
    public Color ColorTreeAccess = Colors.Blue;
    public Color ColorItemAccess = Colors.Green;

    public int PenWidth = 1;

    private Pen PenListAccess = null;
    private Pen PenTreeAccess = null;

    private Pen PenListAccessOptional = null;
    private Pen PenTreeAccessOptional = null;
    private Pen PenItemAccessOptional = null;

    private void Dispose()
    {
        PenListAccess?.Dispose();
        PenListAccess = null;
        PenTreeAccess?.Dispose();
        PenTreeAccess = null;
        PenItemAccessOptional?.Dispose();
        PenItemAccessOptional = null;
        PenListAccessOptional?.Dispose();
        PenListAccessOptional = null;
        PenTreeAccessOptional?.Dispose();
        PenTreeAccessOptional = null;
    }

    private void InitializePen()
    {
        PenListAccess = new Pen(ColorListAccess, PenWidth);
        PenTreeAccess = new Pen(ColorTreeAccess, PenWidth);
        PenItemAccessOptional = new Pen(ColorItemAccess, PenWidth);
        PenListAccessOptional = new Pen(ColorListAccess, PenWidth);
        PenTreeAccessOptional = new Pen(ColorTreeAccess, PenWidth);

        PenItemAccessOptional.DashStyle = DashStyles.Dash;
        PenListAccessOptional.DashStyle = DashStyles.Dash;
        PenTreeAccessOptional.DashStyle = DashStyles.Dash;
    }

    private static bool IsOptional(IParameter param)
    {
        if (!HintOptional) return false;
        return param.Requirement != Requirement.MustExist;
    }

    private void ProcessParams(Canvas canvas, IEnumerable<IParameter> paramList, bool isInput)
    {
        var visibleFrame = canvas.VisibleFrame;
        List<RectangleF>? list = null;

        foreach (var param in paramList)
        {
            var allowPaint = false;

            switch (param.Access)
            {
                case Access.Twig:
                    allowPaint = true;
                    list = IsOptional(param) ? boxesListAccessOptional : boxesListAccess;
                    break;
                case Access.Tree:
                    allowPaint = true;
                    list = IsOptional(param) ? boxesTreeAccessOptional : boxesTreeAccess;
                    break;
                case Access.Item:
                    allowPaint = IsOptional(param);
                    list = boxesItemAccessOptional;
                    break;
                default:
                    // Intentionally left blank
                    break;
            }

            if (allowPaint)
            {
                var box = param.Attributes.Bounds;
                box.Inflate(-1, -1);
                AdjustBoxBySide(ref box, isInput);
                if (visibleFrame.Intersects(box))
                    list?.Add(box);
            }
        }
    }

    private static void AdjustBoxBySide(ref RectangleF box, bool isInput)
    {
        switch (isInput)
        {
            case true:
                box.Inflate(-2, -2);
                break;
            case false:
                box.Inflate(-2, -2);
                break;
        }
    }

    public void Register(Canvas canvas)
    {
        Unregister(canvas);
        InitializePen();
        canvas.AfterPaintObjects += Paint;
    }

    public void Unregister(Canvas canvas)
    {
        canvas.AfterPaintObjects -= Paint;
        Dispose();
    }
}
