using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GH_Util;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Undo;
using Grasshopper.Kernel.Undo.Actions;
using Pancake.GH;
using Pancake.GH.Tweaks;
using Pancake.Utility;
using PancakeNextCore.PancakeMgr;

namespace PancakeNextCore.Helper;

public class ExtendedContextMenu : Feature
{
    private readonly ContextMenuStrip _wireMenu = new ContextMenuStrip();
    private readonly ContextMenuStrip _componentMenu = new ContextMenuStrip();
    private readonly ContextMenuStrip _paramMenu = new ContextMenuStrip();
    private readonly ContextMenuStrip _groupMenu = new ContextMenuStrip();
    private readonly ContextMenuStrip _undefinedMenu = new ContextMenuStrip();

    private bool _enabled;
    private IGH_Param activeSource;
    private IGH_Param activeTarget;
    private IGH_Component activeComponent;

    private void MouseClickEventHandler(object sender, MouseEventArgs e)
    {
        if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift) return;
        if ((Control.ModifierKeys & Keys.Control) != Keys.Control) return;

        if (e.Button != MouseButtons.Right) return;

        var doc = Instances.ActiveCanvas?.Document;
        if (doc == null)
            return;

        try
        {
            var pointf = Instances.ActiveCanvas.Viewport.UnprojectPoint(e.Location);

            var searchRadius = 14 / Instances.ActiveCanvas.Viewport.Zoom;

            if (doc.FindWireAt(pointf, searchRadius, ref activeSource, ref activeTarget))
            {
                _wireMenu.Show((Control)sender, e.Location);
                return;
            }

            foreach (var obj in doc.Objects)
            {
                switch (obj)
                {
                    case IGH_Param param
                        when param.Kind == GH_ParamKind.floating && param.Attributes.Bounds.Contains(pointf):

                        activeSource = param;
                        activeComponent = null;
                        _componentMenu.Show((Control)sender, e.Location);
                        return;

                    case IGH_Component component
                        when component.Attributes.Bounds.Contains(pointf):

                        foreach (var param in component.Params.Input.Concat(component.Params.Output))
                        {
                            if (!param.Attributes.Bounds.Contains(pointf)) continue;

                            activeComponent = component;
                            activeSource = param;
                            _paramMenu.Show((Control)sender, e.Location);
                            return;
                        }

                        activeComponent = component;
                        _componentMenu.Show((Control)sender, e.Location);
                        return;
                }
            }

            if (doc.SelectedCount != 0)
            {
                _groupMenu.Show((Control)sender, e.Location);
                return;
            }

            _undefinedMenu.Show((Control)sender, e.Location);
        }
        catch
        {
            // Something bad happened.
        }

    }

    public override void OnLoad()
    {
        try
        {
            _enabled = true;

            if (_wireMenu == null || _wireMenu.Items.Count == 0)
                InitMenu();

            Instances.ActiveCanvas.MouseUp += MouseClickEventHandler;
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

        menu.AddEntry(Strings.Delete, HandlerDelete);
        menu.AddSeparator();

        menu.AddEntry(Strings.Source, HandlerToSrc);
        menu.AddEntry(Strings.Target, HandlerToDest);

        menu.AddSeparator();
        menu.AddEntry(Strings.Normal, HandleSetNormal);
        menu.AddEntry(Strings.Faint, HandleSetFaint);
        menu.AddEntry(Strings.SetAsHidden, HandleSetHidden);

        menu = new MenuConstructor(_componentMenu.Items);

        menu.AddLabel("Pancake", "contextMain");
        menu.AddSeparator();

        menu.AddSeparator();
        menu.AddEntry(Strings.InputWiresNormal, HandleSetNormalAll);
        menu.AddEntry(Strings.InputWiresFaint, HandleSetFaintAll);
        menu.AddEntry(Strings.InputWiresHidden, HandleSetHiddenAll);

        menu.AddSeparator();
        menu.AddEntry("Set all output wires as normal", HandleSetOutputNormalAll);
        menu.AddEntry("Set all output wires as faint", HandleSetOutputFaintAll);
        menu.AddEntry("Set all output wires as hidden", HandleSetOutputHiddenAll);

        menu.AddSeparator();

        menu.AddEntry(Strings.DisconnectAllInputs, HandleDisconnectAllInputs);
        menu.AddEntry(Strings.DisconnectAllOutputs, HandleDisconnectAllOutputs);

        menu.AddSeparator();
        menu.AddEntry(Strings.Inspect, HandleInspectDocObject);

        menu = new MenuConstructor(_paramMenu.Items);

        menu.AddLabel("Pancake", "contextMain");
        menu.AddSeparator();

        menu.AddEntry(Strings.InputWiresNormal, HandleSetNormalAll);
        menu.AddEntry(Strings.InputWiresFaint, HandleSetFaintAll);
        menu.AddEntry(Strings.InputWiresHidden, HandleSetHiddenAll);

        menu.AddSeparator();
        menu.AddEntry("Set all output wires as normal", HandleSetOutputNormalAll);
        menu.AddEntry("Set all output wires as faint", HandleSetOutputFaintAll);
        menu.AddEntry("Set all output wires as hidden", HandleSetOutputHiddenAll);

        menu.AddSeparator();
        menu.AddEntry(Strings.InspectComponent, HandleInspectDocObject);
        menu.AddEntry(Strings.InspectParameter, HandleInspectParameter);

        menu = new MenuConstructor(_groupMenu.Items);

        menu.AddLabel("Pancake", "contextMain");
        menu.AddSeparator();

        menu.AddEntry(Strings.ProfileSelectedComponents, HandleCountTime);
        menu.AddEntry(Strings.ProfileSelectedComponentsRepeated, HandleCountTimeRepeated);

        menu.AddSeparator();

        menu.AddEntry(Strings.InputWiresNormal, HandleSetNormalSelected);
        menu.AddEntry(Strings.InputWiresFaint, HandleSetFaintSelected);
        menu.AddEntry(Strings.InputWiresHidden, HandleSetHiddenSelected);

        menu = new MenuConstructor(_undefinedMenu.Items);

        menu.AddLabel("Pancake", "contextMain");
        menu.AddSeparator();
        menu.AddEntry(Strings.ActionsDefinedThisKindObjectS, (x, y) => { });
    }

    private void HandleSetOutputHiddenAll(object sender, EventArgs e)
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
    }

    private void HandleInspectParameter(object sender, EventArgs e)
    {
        DbgInfo.ShowDocObjInfo(activeSource);
    }

    private void HandleInspectDocObject(object sender, EventArgs e)
    {
        DbgInfo.ShowDocObjInfo(activeComponent ?? activeSource as IGH_DocumentObject);
    }

    private void HandleSetHiddenSelected(object sender, EventArgs e)
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
    }

    public override void OnUnload()
    {
        try
        {
            Instances.ActiveCanvas.MouseUp -= MouseClickEventHandler;
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

    public static string Name = "Advanced wire";

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
