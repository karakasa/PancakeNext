using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Eto.Forms;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using PancakeNextCore.GH;
using PancakeNextCore.PancakeMgr;

namespace PancakeNextCore.Helper;

public class ExtendedContextMenu : Feature
{
    private readonly ContextMenu _wireMenu = new();
    private readonly ContextMenu _componentMenu = new();
    private readonly ContextMenu _paramMenu = new();
    private readonly ContextMenu _groupMenu = new();

    private bool _enabled;
    private IParameter? activeParameter;
    private IDocumentObject? activeObject;
    private WireEnds? activeWire;

    const Keys ExpectedModifier = Keys.Control | Keys.Shift;
    const MouseButtons ExpectedButton = MouseButtons.Primary;

    private void MouseClickEventHandler(object? sender, MouseEventArgs e)
    {
        try
        {
            if (Editor.Instance?.Canvas is not { } canvas) return;
            if (canvas.Document is not { } doc) return;

            if ((Keyboard.Modifiers & ExpectedModifier) != ExpectedModifier) return;
            if ((e.Buttons & ExpectedButton) != ExpectedButton) return;

            var mappedEvent = e.MapToContent(canvas);
            var pick = canvas.ResolvePick(mappedEvent.Location, false, true, true, true, false);

            switch (pick.Kind)
            {
                case Pick.Wire:
                    e.Handled = true;
                    HandleWire(doc, pick.WireUnderPick);
                    break;
                case Pick.Inlet:
                    e.Handled = true;
                    HandleParameterInComponent(doc.Objects.FindParameter(pick.InletUnderPick));
                    break;
                case Pick.Outlet:
                    e.Handled = true;
                    HandleParameterInComponent(doc.Objects.FindParameter(pick.OutletUnderPick));
                    break;
                case Pick.BackgroundObject:
                case Pick.ForegroundObject:
                    e.Handled = true;
                    HandleStandaloneObject(doc.Objects.Find(pick.ObjectUnderPick));
                    break;
                default:
                    return;
            }
        }
        catch
        {
            // Something bad happened.
        }

    }

    private void HandleParameterInComponent(IParameter? param)
    {
        if (param is null) return;
        activeParameter = param;

        _paramMenu.Show();
    }

    private void HandleStandaloneObject(IDocumentObject? obj)
    {
        if (obj is null) return;
        activeObject = obj;

        _componentMenu.Show();
    }

    private void HandleWire(Document doc, WireEnds obj)
    {
        activeWire = obj;
        _wireMenu.Show();
    }

    public override void OnLoad()
    {
        try
        {
            _enabled = true;

            if (_wireMenu == null || _wireMenu.Items.Count == 0)
                InitMenu();

            Editor.Instance.Canvas.MouseUp += MouseClickEventHandler;
        }
        catch
        {
            // ignored
        }
    }

    private void InitMenu()
    {
        var menu = new MenuConstructor(_wireMenu.Items);

        menu.AddLabel("Pancake", "contextMain");
        menu.AddSeparator();

        // menu.AddEntry(Strings.Delete, HandlerDelete);
        menu.AddSeparator();

        //menu.AddEntry(Strings.Source, HandlerToSrc);
        //menu.AddEntry(Strings.Target, HandlerToDest);

        //menu.AddSeparator();
        //menu.AddEntry(Strings.Normal, HandleSetNormal);
        //menu.AddEntry(Strings.Faint, HandleSetFaint);
        //menu.AddEntry(Strings.SetAsHidden, HandleSetHidden);

        menu = new MenuConstructor(_componentMenu.Items);

        menu.AddLabel("Pancake", "contextMain");
        menu.AddSeparator();

        menu.AddSeparator();
        //menu.AddEntry(Strings.InputWiresNormal, HandleSetNormalAll);
        //menu.AddEntry(Strings.InputWiresFaint, HandleSetFaintAll);
        //menu.AddEntry(Strings.InputWiresHidden, HandleSetHiddenAll);

        //menu.AddSeparator();
        //menu.AddEntry("Set all output wires as normal", HandleSetOutputNormalAll);
        //menu.AddEntry("Set all output wires as faint", HandleSetOutputFaintAll);
        //menu.AddEntry("Set all output wires as hidden", HandleSetOutputHiddenAll);

        //menu.AddSeparator();

        //menu.AddEntry(Strings.DisconnectAllInputs, HandleDisconnectAllInputs);
        //menu.AddEntry(Strings.DisconnectAllOutputs, HandleDisconnectAllOutputs);

        menu.AddSeparator();
        menu.AddEntry(Strings.Inspect, HandleInspectDocObject);

        menu = new MenuConstructor(_paramMenu.Items);

        menu.AddLabel("Pancake", "contextMain");
        menu.AddSeparator();

        //menu.AddEntry(Strings.InputWiresNormal, HandleSetNormalAll);
        //menu.AddEntry(Strings.InputWiresFaint, HandleSetFaintAll);
        //menu.AddEntry(Strings.InputWiresHidden, HandleSetHiddenAll);

        //menu.AddSeparator();
        //menu.AddEntry("Set all output wires as normal", HandleSetOutputNormalAll);
        //menu.AddEntry("Set all output wires as faint", HandleSetOutputFaintAll);
        //menu.AddEntry("Set all output wires as hidden", HandleSetOutputHiddenAll);

        menu.AddSeparator();
        menu.AddEntry(Strings.InspectComponent, HandleInspectDocObject);
        menu.AddEntry(Strings.InspectParameter, HandleInspectParameter);

        menu = new MenuConstructor(_groupMenu.Items);

        menu.AddLabel("Pancake", "contextMain");
        menu.AddSeparator();

        //menu.AddEntry(Strings.ProfileSelectedComponents, HandleCountTime);
        //menu.AddEntry(Strings.ProfileSelectedComponentsRepeated, HandleCountTimeRepeated);

        //menu.AddSeparator();

        //menu.AddEntry(Strings.InputWiresNormal, HandleSetNormalSelected);
        //menu.AddEntry(Strings.InputWiresFaint, HandleSetFaintSelected);
        //menu.AddEntry(Strings.InputWiresHidden, HandleSetHiddenSelected);
    }

    /*private void HandleSetOutputHiddenAll(object sender, EventArgs e)
    {
        SetWireStyle(GH_ParamWireDisplay.hidden, false);
    }

    private void HandleSetOutputFaintAll(object sender, EventArgs e)
    {
        SetWireStyle(GH_ParamWireDisplay.faint, false);
    }

    private void HandleSetOutputNormalAll(object sender, EventArgs e)
    {
        SetWireStyle(GH_ParamWireDisplay.@default, false);
    }

    private void HandleDisconnectAllInputs(object sender, EventArgs e)
    {
        if (activeComponent == null) return;

        var doc = activeComponent.OnPingDocument();
        if (doc == null) return;
        doc.UndoUtil.RecordGenericObjectEvent("Disconnect wires", activeComponent);

        activeComponent.Params.Input.DoForEach(p => p.RemoveAllSources());
        activeComponent.ExpireSolution(true);
        Instances.ActiveCanvas?.Invalidate();
    }

    private void HandleDisconnectAllOutputs(object sender, EventArgs e)
    {
        if (activeComponent == null) return;

        var doc = activeComponent.OnPingDocument();
        if (doc == null) return;
        doc.UndoUtil.RecordGenericObjectEvent("Disconnect wires", activeComponent);
        activeComponent.Params.Output.DoForEach(p =>
        {
            foreach (var recipient in p.Recipients.ToArray())
                recipient.RemoveSource(p);
        });

        activeComponent.ExpireSolution(true);
        Instances.ActiveCanvas?.Invalidate();
    }

    internal static void ShowTimeCountReport(bool reprofile)
    {
        var doc = Instances.ActiveCanvas?.Document;
        if (doc == null || doc.SelectedCount == 0)
            return;

        if (reprofile)
            Performance.ProfileRepeated(doc, doc.SelectedObjects().OfType<IGH_ActiveObject>());
        else
            Performance.ShowObjectGroupTimeReport(doc, doc.SelectedObjects().OfType<IGH_ActiveObject>());
    }

    private void HandleCountTimeRepeated(object sender, EventArgs e)
    {
        ShowTimeCountReport(true);
    }

    private void HandleCountTime(object sender, EventArgs e)
    {
        ShowTimeCountReport(false);
    }*/

    private void HandleInspectParameter()
    {
        DbgInfo.ShowDocObjInfo(activeParameter);
    }

    private void HandleInspectDocObject()
    {
        DbgInfo.ShowDocObjInfo(activeObject ?? activeParameter as IDocumentObject);
    }

    /*private void HandleSetHiddenSelected(object sender, EventArgs e)
    {
        SetWireStyleSelected(GH_ParamWireDisplay.hidden);
    }

    private void HandleSetFaintSelected(object sender, EventArgs e)
    {
        SetWireStyleSelected(GH_ParamWireDisplay.faint);
    }

    private void HandleSetNormalSelected(object sender, EventArgs e)
    {
        SetWireStyleSelected(GH_ParamWireDisplay.@default);
    }
    private void HandleSetHiddenAll(object sender, EventArgs e)
    {
        SetWireStyle(GH_ParamWireDisplay.hidden);
    }

    private void HandleSetFaintAll(object sender, EventArgs e)
    {
        SetWireStyle(GH_ParamWireDisplay.faint);
    }

    private void HandleSetNormalAll(object sender, EventArgs e)
    {
        SetWireStyle(GH_ParamWireDisplay.@default);
    }

    private void HandlerToSrc(object sender, EventArgs e)
    {
        GhGui.ZoomToObjects(new[] { activeSource.Attributes.GetTopLevel.DocObject });
    }

    private void HandlerToDest(object sender, EventArgs e)
    {
        GhGui.ZoomToObjects(new[] { activeTarget.Attributes.GetTopLevel.DocObject });
    }

    private static void SetWireStyle(IGH_Param target, GH_ParamWireDisplay style, bool inputSide = true)
    {
        if (inputSide)
        {
            if (target.WireDisplay == style)
                return;

            var doc = Instances.ActiveCanvas.Document;
            doc.UndoUtil.RecordEvent("Pancake.Core.SetWireStyle", new GH_WireDisplayAction(target));
            target.WireDisplay = style;
            Instances.RedrawCanvas();
        }
        else
        {
            SetWireStyle(target.Recipients, style);
        }
    }
    private void SetWireStyleSelected(GH_ParamWireDisplay style)
    {
        var doc = Instances.ActiveCanvas.Document;
        var undos = new List<GH_UndoAction>();

        foreach (var obj in doc.Objects
            .Where(o => (o is IGH_Param || o is IGH_Component) && (o.Attributes?.Selected ?? false))
            )
        {
            switch (obj)
            {
                case IGH_Param param:
                    undos.Add(new GH_WireDisplayAction(param));
                    param.WireDisplay = style;
                    break;
                case IGH_Component comp:
                    foreach (var param in comp.Params.Input)
                    {
                        undos.Add(new GH_WireDisplayAction(param));
                        param.WireDisplay = style;
                    }
                    break;
            }
        }

        if (undos.Count == 0)
            return;

        doc.UndoUtil.RecordEvent("Pancake.Core.SetComponentWireStyle", undos);
        Instances.RedrawCanvas();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="style"></param>
    /// <param name="inputSide">True as input, false as output.</param>
    private void SetWireStyle(GH_ParamWireDisplay style, bool inputSide = true)
    {
        if (activeComponent == null)
        {
            SetWireStyle(activeSource, style, inputSide);
            return;
        }

        var paramList = inputSide ?
            activeComponent.Params.Input
            : activeComponent.Params.Output.SelectMany(o => o.Recipients);

        SetWireStyle(paramList, style);
    }

    private static void SetWireStyle(IEnumerable<IGH_Param> paramList, GH_ParamWireDisplay style)
    {
        var doc = Instances.ActiveCanvas.Document;
        var undos = new List<GH_UndoAction>();

        foreach (var param in paramList)
        {
            if (param.WireDisplay == style)
                continue;

            undos.Add(new GH_WireDisplayAction(param));
            param.WireDisplay = style;
        }

        if (undos.Count > 0)
        {
            doc.UndoUtil.RecordEvent("Pancake.Core.SetComponentWireStyle", undos);
            Instances.RedrawCanvas();
        }
    }

    private void HandleSetHidden(object sender, EventArgs e)
    {
        SetWireStyle(activeTarget, GH_ParamWireDisplay.hidden);
    }

    private void HandleSetNormal(object sender, EventArgs e)
    {
        SetWireStyle(activeTarget, GH_ParamWireDisplay.@default);
    }

    private void HandleSetFaint(object sender, EventArgs e)
    {
        SetWireStyle(activeTarget, GH_ParamWireDisplay.faint);
    }

    private void HandlerDelete(object sender, EventArgs e)
    {
        var doc = Instances.ActiveCanvas.Document;
        doc.UndoUtil.RecordWireEvent("Pancake.Core.DeleteWire", activeTarget);
        activeTarget.RemoveSource(activeSource);
        activeTarget.ExpireSolution(true);
    }*/

    public override void OnUnload()
    {
        try
        {
            if (Editor.Instance?.Canvas is { } canvas)
                canvas.MouseUp -= MouseClickEventHandler;

            _enabled = false;
        }
        catch
        {
            // ignored
        }
    }

    public override bool IsEffective()
    {
        return _enabled;
    }

    public const string Name = "Advanced wire";

    public override string GetName()
    {
        return Name;
    }

    public override FeatureManager.LoadStage GetExpectedLoadStage()
    {
        return FeatureManager.LoadStage.UI;
    }

    public override bool IsDefaultEnabled()
    {
        return false;
    }
}
