using Eto.Drawing;
using Eto.Forms;
using Grasshopper;
using Grasshopper.Kernel.Special;
using Pancake.Dataset;
using Pancake.GH.Tweaks;
using PancakeNextCore.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pancake.UI.EtoForms;

internal partial class FormPerformanceAnalyzer : Form
{
    private EnumDropDown<GroupCriteria> _comboBoxClassifier;
    private CheckBox _checkBoxAutoRefresh;
    private CheckBox _checkBoxFocusSelected;
    private Button _buttonAdvanced;
    private Button _buttonRefreshSelection;
    private GridView _gridView;

    private ObservableCollection<PerformanceAnalyzerEntry> _entries = new();

    private bool _suspendEvent = true;
    public FormPerformanceAnalyzer()
    {
        InitializeComponents();
        RegisterDocChangeEvents();
    }

    public void ResumeEvents()
    {
        _suspendEvent = false;
    }
    private static string GroupCriteriaToString(GroupCriteria criteria)
    {
        switch (criteria)
        {
            case GroupCriteria.ByComponent:
                return "By Component";
            case GroupCriteria.ByGroup:
                return "By Group";
            case GroupCriteria.ByCategory:
                return "By Category";
            default:
                return string.Empty;
        }
    }
    private void InitializeComponents()
    {
        ClientSize = new Size(450, 400);
        Title = "Performance Analyzer";

        _comboBoxClassifier = new EnumDropDown<GroupCriteria>
        {
            GetText = GroupCriteriaToString,
            SelectedValue = GroupCriteria.ByComponent
        };

        _comboBoxClassifier.SelectedValueChanged += OnGroupingCriteriaChanged;

        _checkBoxAutoRefresh = new CheckBox
        {
            Text = "Auto refresh"
        };

        _checkBoxAutoRefresh.CheckedBinding.Bind(
            () => AutoRefresh,
            v => AutoRefresh = v ?? false
            );

        _checkBoxFocusSelected = new CheckBox
        {
            Text = "Focus selected"
        };

        _checkBoxFocusSelected.CheckedChanged += OnFocusSelectedChanged;

        _buttonAdvanced = new Button
        {
            Text = "Advanced...",
            Enabled = false
        };

        _buttonRefreshSelection = new Button
        {
            Text = "Refresh selection"
        };

        _buttonRefreshSelection.Click += OnRefreshClick;

        _gridView = new GridView
        {
            DataStore = _entries,
            GridLines = GridLines.Both,
            AllowMultipleSelection = false,
            Cursor = Cursors.Pointer,
            Columns =
            {
                new GridColumn
                {
                    DataCell = new ImageTextCell
                    {
                        ImageBinding = Binding.Delegate<PerformanceAnalyzerEntry, Image>(GetComponentImage),
                        TextBinding = Binding.Property<PerformanceAnalyzerEntry, string>(static e => e.ObjectName),
                    },
                    HeaderText = "Object",
                    AutoSize = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell
                    {
                        Binding = Binding.Property<PerformanceAnalyzerEntry, string>(static e => e.DisplayTime),
                        TextAlignment = TextAlignment.Right
                    },
                    HeaderText = "Time",
                    AutoSize = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell
                    {
                        Binding = Binding.Property<PerformanceAnalyzerEntry, string>(static e => e.DisplayPercentage),
                        TextAlignment = TextAlignment.Right
                    },
                    HeaderText = "%",
                    AutoSize = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell
                    {
                        Binding = Binding.Property<PerformanceAnalyzerEntry, string>(static e => e.DisplayRunCount),
                        TextAlignment = TextAlignment.Right
                    },
                    HeaderText = "Iteration",
                    AutoSize = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell
                    {
                        Binding = Binding.Property<PerformanceAnalyzerEntry, string>(static e => e.DisplayTimePerRun),
                        TextAlignment = TextAlignment.Right
                    },
                    HeaderText = "Time per run",
                    AutoSize = true
                },
            }
        };

        _gridView.SelectedRowsChanged += OnSelectedRowChanged;

        Content = new TableLayout
        {
            Spacing = new Size(5, 5),
            Padding = new Padding(10),
            Rows =
            {
                new TableRow(
                    new TableLayout
                    {
                        Spacing = new Size(5, 5),
                        Padding = new Padding(0),
                        Rows =
                        {
                            new TableRow(
                                _comboBoxClassifier,
                                _buttonAdvanced
                            ),
                            new TableRow
                            (
                                new TableLayout
                                {
                                    Spacing = new Size(5, 5),
                                    Padding = new Padding(0),
                                    Rows =
                                    {
                                        new TableRow(
                                            _checkBoxAutoRefresh,
                                            _checkBoxFocusSelected
                                            )
                                    }
                                },
                                _buttonRefreshSelection
                            )
                        }
                    }
                ),
                new TableRow(
                    _gridView
                )
            }
        };
    }

    private void OnSelectedRowChanged(object sender, EventArgs e)
    {
        if (_suspendEvent) return;

        if (_gridView.SelectedItem is not PerformanceAnalyzerEntry entry) return;

        var guid = entry.ObjectId;
        if (guid == Guid.Empty) return;

        var doc = Instances.ActiveCanvas?.Document;
        if (doc == null) return;

        var docObj = doc.FindObject(guid, true);
        if (docObj == null) return;

        if (docObj is GH_Group group)
        {
            GhGui.ZoomToObjects(group.Objects());
        }
        else
        {
            GhGui.ZoomToObject(docObj);
        }
    }

    private void OnRefreshClick(object sender, EventArgs e)
    {
        RefreshContent();
    }

    private void OnFocusSelectedChanged(object sender, EventArgs e)
    {
        if (_suspendEvent) return;

        FocusSelected = _checkBoxFocusSelected.Checked ?? false;
        _buttonRefreshSelection.Enabled = FocusSelected;
        RefreshContent();
    }

    private void OnGroupingCriteriaChanged(object sender, EventArgs e)
    {
        if (_suspendEvent) return;

        Grouping = _comboBoxClassifier.SelectedValue;
        RefreshContent();
    }
}