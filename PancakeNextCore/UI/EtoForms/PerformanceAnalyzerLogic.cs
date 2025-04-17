using Eto.Drawing;
using Grasshopper;
using Grasshopper.Kernel;
using Pancake.Dataset;
using Pancake.Helper;
using Pancake.Utility;
using PancakeNextCore.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pancake.UI.EtoForms;

internal partial class FormPerformanceAnalyzer : IFormPerformanceAnalyzer<Image>
{
    private PerformanceSnapshot _snapshot;
    private HashSet<Guid> _preselected = null;
    private static EtoComponentIconCacher _componentIconCacher = new();
    private List<Image> _componentIcons = new();

    private static readonly PerformanceAnalyzerEntry EmptyRow = new()
    {
        DisplayPercentage = String.Empty,
        DisplayRunCount = String.Empty,
        DisplayTime = String.Empty,
        DisplayTimePerRun = String.Empty,
        ObjectName = String.Empty,
        ImageId = PerformanceAnalyzerEntry.NO_IMAGE
    };
    public bool FocusSelected { get; set; } = false;
    public static bool AutoRefresh { get; set; } = true;

    public GroupCriteria Grouping { get; set; } = GroupCriteria.ByComponent;

    public void AddEntry(PerformanceAnalyzerEntry entry)
    {
        _entries.Add(entry);
    }

    public int AddEntryImage(Image img)
    {
        _componentIcons.Add(img);
        return _componentIcons.Count - 1;
    }

    public void AddSpecialEntry(SpecialEntryType type, PerformanceAnalyzerEntry entry)
    {
        if (type == SpecialEntryType.EmptyRow)
        {
            AddEntry(EmptyRow);
        }
        else
        {
            AddEntry(entry);
        }
    }

    public void BeginVisualize()
    {
        _componentIcons.Clear();
        _entries.Clear();
    }

    public void EndVisualize()
    {
    }
    private Image GetComponentImage(PerformanceAnalyzerEntry entry)
    {
        if (entry.ImageId == PerformanceAnalyzerEntry.NO_IMAGE)
            return null;

        return _componentIcons[entry.ImageId];
    }

    private void RefreshContent()
    {
        PerformanceAnalyzerPresenter.RefreshContent(
            this,
            _componentIconCacher.Get,
            _snapshot,
            _preselected);
    }
    public static FormPerformanceAnalyzer CreateFromSnapshot(PerformanceSnapshot snapshot, bool focusSelected = false)
    {
        var form = new FormPerformanceAnalyzer();
        form.FocusSelected = focusSelected;
        form._checkBoxAutoRefresh.Enabled = false;
        form._checkBoxFocusSelected.Checked = focusSelected;
        form._buttonRefreshSelection.Enabled = focusSelected;
        form.ShowSnapshot(snapshot);

        form.ResumeEvents();

        return form;
    }

    public static FormPerformanceAnalyzer CreateFromSnapshot(PerformanceSnapshot snapshot, IEnumerable<Guid> focused)
    {
        var form = new FormPerformanceAnalyzer();
        form._checkBoxAutoRefresh.Enabled = form._checkBoxFocusSelected.Enabled = false;
        form._checkBoxFocusSelected.Checked = true;
        form.FocusSelected = true;
        form._buttonRefreshSelection.Enabled = false;
        form._preselected = new HashSet<Guid>(focused);
        form.ShowSnapshot(snapshot);

        form.ResumeEvents();

        return form;
    }

    public void ShowSnapshot(PerformanceSnapshot snapshot)
    {
        ResumeEvents();

        _snapshot = snapshot;
        RefreshContent();
    }
    private static bool EventRegistered = false;
    private static void RegisterDocChangeEvents()
    {
        if (Config.SafeMode)
            return;

        if (EventRegistered)
            return;

        var canvas = Instances.ActiveCanvas;
        if (canvas == null)
            return;

        canvas.DocumentChanged += Canvas_DocumentChanged;
        var doc = canvas.Document;
        if (doc != null)
        {
            doc.SolutionEnd -= OnSolutionEnd;
            doc.SolutionEnd += OnSolutionEnd;
        }

        EventRegistered = true;
    }
    private static void Canvas_DocumentChanged(Grasshopper.GUI.Canvas.GH_Canvas sender, Grasshopper.GUI.Canvas.GH_CanvasDocumentChangedEventArgs e)
    {
        if (e.OldDocument != null)
            e.OldDocument.SolutionEnd -= OnSolutionEnd;

        if (e.NewDocument != null)
            e.NewDocument.SolutionEnd += OnSolutionEnd;
    }
    private static void OnSolutionEnd(object sender, GH_SolutionEventArgs e)
    {
        if (!AutoRefresh)
            return;

        if (PersistentEtoForm<FormPerformanceAnalyzer>.TryGet(out var form))
        {
            if (e.Document != null)
                form.NewSnapshotAndShow(e.Document);
        }
    }

    private void NewSnapshotAndShow(GH_Document doc)
    {
        ShowSnapshot(PerformanceSnapshot.CreateFromDoc(doc));
    }
}
