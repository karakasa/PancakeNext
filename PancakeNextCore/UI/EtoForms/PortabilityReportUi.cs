using Eto.Drawing;
using Eto.Forms;
using Pancake.Modules.PortabilityCheckerActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pancake.UI.EtoForms;

internal partial class FormPortabilityReport : Form
{
    public FormPortabilityReport()
    {
        InitializeComponents();
        InitializeConfigurations();
    }

    private Label _labelTarget;
    private DropDown _dropDownTarget;
    private TreeGridView _treeView;
    private TextArea _textAreaInformation;

    private readonly TreeGridItemCollection _treeItems = new();

    private Button[] _actionButtons;
    private void InitializeComponents()
    {
        Resizable = true;
        ClientSize = new Size(650, 450);
        Title = "Portability Report";

        _labelTarget = new Label
        {
            Text = "Target"
        };

        _dropDownTarget = new DropDown();

        _dropDownTarget.SelectedValueChanged += OnTargetChanged;

        _treeView = new TreeGridView
        {
            DataStore = _treeItems,
            AllowMultipleSelection = false,
            Columns = {new GridColumn
            {
                HeaderText = "",
                AutoSize = true,
                DataCell = new TextBoxCell(0)
            } }
        };

        _treeView.SelectedItemChanged += OnSelectedItemChanged;

        _textAreaInformation = new TextArea
        {
            Height = 100,
            ReadOnly = true,
            Text = Strings.SelectASubitemForMoreInformation
        };

        var dynamicLayout = new DynamicLayout();

        dynamicLayout.BeginVertical(new Padding(0, 10), new Size(0, 5), true, false);

        foreach (var it in CreateActionButtons())
        {
            dynamicLayout.Add(it, true, false);
        }

        dynamicLayout.AddSpace();
        dynamicLayout.EndVertical();

        Content = new TableLayout
        {
            Padding = new Padding(10),
            Spacing = new Size(5, 5),
            Rows =
            {
                new TableRow (new TableLayout { Padding = new Padding(10, 0, 10, 0),Spacing = new Size(5, 5),Rows = {new TableRow(_labelTarget, _dropDownTarget)} }),
                new TableRow (new TableLayout { Padding = new Padding(10),Spacing = new Size(15, 5),Rows = {
                        new TableRow(
                            new TableCell(_treeView, true)
                            , new TableLayout
                        {
                                Width = 200,
                            //Padding = new Padding(10),
                            Rows =
                            {
                                new TableRow(_textAreaInformation),
                                new TableRow(dynamicLayout)
                            }
                        })
                    } })
            }
        };
    }

    private void OnSelectedItemChanged(object sender, EventArgs e)
    {
        for (var i = 0; i < _actions.Length; i++)
        {
            _actionButtons[i].Enabled = _actions[i].SelectionNotRequired;
        }

        if (_treeView.SelectedItem is not TreeGridItem si) return;

        if (!int.TryParse(si.Tag as string, out var index) || index < 0 || index >= Results.Length)
        {
            _textAreaInformation.Text = Strings.SelectASubitemForMoreInformation;
            return;
        }

        _textAreaInformation.Text = Results[index].Description ?? Strings.DetailedInformationUnavailable;

        for (var i = 0; i < _actions.Length; i++)
        {
            _actionButtons[i].Enabled = _actions[i].ShouldEnableOnSelection(Results[index]);
        }
    }

    private Button[] CreateActionButtons()
    {
        _actionButtons = _actions.Select(
            act => {
                var button = new Button { Text = act.ButtonText, Tag = act, Width = -1, Height = 30 };
                button.Click += OnActionButtonClicked;
                return button;
            }).ToArray();

        return _actionButtons;
    }

    private void OnActionButtonClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.Tag is not IPortabilityCheckerAction action) return;

        if (action.SelectionNotRequired)
        {
            action.DoAction(this, default);
            return;
        }

        if (_treeView.SelectedItem is not TreeGridItem si) return;

        if (!int.TryParse(si.Tag as string, out var index) || index < 0 || index >= Results.Length)
            return;

        action.DoAction(this, Results[index]);
    }

    private void OnTargetChanged(object sender, EventArgs e)
    {
        RefreshContent();
    }
}
