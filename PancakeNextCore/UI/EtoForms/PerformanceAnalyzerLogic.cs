using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using PancakeNextCore.Dataset;
using PancakeNextCore.Helper;
using PancakeNextCore.UI;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.UI.EtoForms;

internal partial class FormPerformanceAnalyzer : IFormPerformanceAnalyzer
{
    private readonly struct ComponentIconAccessor : IStructFunc<IDocumentObject, Image>
    {
        public Image Invoke(IDocumentObject param)
        {
            var icon = param.Icon;
            return icon.DrawToBitmap(new Size(24, 24), 0, Colors.Transparent);
        }
    }
    private sealed class ComponentComparer : IEqualityComparer<IDocumentObject>
    {
        public static ComponentComparer Instance = new();
        public bool Equals(IDocumentObject? x, IDocumentObject? y) => x?.GetType() == y?.GetType();

        public int GetHashCode(IDocumentObject obj) => obj?.GetHashCode() ?? 0;
    }

    private PerformanceSnapshot _snapshot;
    private HashSet<Guid>? _preselected = null;
    private static StructFuncCache<ComponentIconAccessor, IDocumentObject, Image> _componentIconCacher = new(default, ComponentComparer.Instance);
    private readonly List<Image> _componentIcons = [];

    private static readonly PerformanceAnalyzerEntry EmptyRow = new()
    {
        DisplayPercentage = String.Empty,
        // DisplayRunCount = String.Empty,
        DisplayTime = String.Empty,
        // DisplayTimePerRun = String.Empty,
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

    public void AddSpecialEntry(SpecialEntryType type, PerformanceAnalyzerEntry? entry)
    {
        if (type == SpecialEntryType.EmptyRow || entry is null)
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
    private Image? GetComponentImage(PerformanceAnalyzerEntry entry)
    {
        if (entry.ImageId == PerformanceAnalyzerEntry.NO_IMAGE)
            return null;

        return _componentIcons[entry.ImageId];
    }

    private void RefreshContent()
    {
        PerformanceAnalyzerPresenter.RefreshContent(
            this,
            static o => _componentIconCacher[o],
            _snapshot,
            _preselected);
    }
    public static FormPerformanceAnalyzer CreateFromSnapshot(PerformanceSnapshot snapshot, bool focusSelected = false)
    {
        var form = new FormPerformanceAnalyzer { FocusSelected = focusSelected };

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
        form._preselected = new(focused);
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

        var canvas = Editor.Instance?.Canvas;
        if (canvas == null)
            return;

        canvas.DocumentChanged += Canvas_DocumentChanged;
        Canvas_DocumentChanged(canvas, EventArgs.Empty);

        EventRegistered = true;
    }

    //protected override void OnClosed(EventArgs e)
    //{
    //    var canvas = Editor.Instance?.Canvas;
    //    if (canvas == null)
    //        return;

    //    canvas.DocumentChanged -= Canvas_DocumentChanged;
    //    if (canvas.Document is { } doc)
    //        UnregisterEvents(doc);

    //    base.OnClosed(e);
    //}

    private static void Canvas_DocumentChanged(object? sender, EventArgs e)
    {
        var doc = (sender as Canvas)?.Document;
        if (doc is null) return;

        UnregisterEvents(doc);
        doc.StateChanged += OnDocumentStateChanged;
    }

    private static void UnregisterEvents(Document doc)
    {
        doc.StateChanged -= OnDocumentStateChanged;
        doc.Solution.SolutionCompleted -= OnSolutionEnd;
    }
    private static void OnDocumentStateChanged(object? sender, DocumentStateEventArgs e)
    {
        if (sender is not Document doc) return;

        if (doc.State != DocumentState.Active)
            UnregisterEvents(doc);
    }

    private static void OnSolutionEnd(object? sender, SolutionEventArgs e)
    {
        if (!AutoRefresh)
            return;

        if (PersistentEtoForm<FormPerformanceAnalyzer>.TryGet(out var form))
        {
            if (e.Document != null)
                form.NewSnapshotAndShow(e.Document);
        }
    }

    private void NewSnapshotAndShow(Document doc)
    {
        var snapshot = PerformanceSnapshot.CreateFromDoc(doc);
        if (snapshot is not null)
            ShowSnapshot(snapshot);
    }
}
