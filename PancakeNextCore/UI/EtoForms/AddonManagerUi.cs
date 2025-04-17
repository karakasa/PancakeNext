using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pancake.UI.EtoForms;

internal partial class FormAddonManager : Form
{
    private SearchBox _textBoxSearch;
    private ListBox _listBoxPluginList;
    private GridView _gridViewInfo;
    private GridView _gridViewObjects;
    private GridView _gridViewFiles;
    public FormAddonManager()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        ClientSize = new Size(1200, 600);
        Title = "Addon Manager";

        _textBoxSearch = new SearchBox();

        _textBoxSearch.TextChanged += OnSearchBoxTextChanged;

        _listBoxPluginList = new ListBox
        {
            DataStore = _plugins,
            ItemTextBinding = Binding.Property<PluginInfo, string>(static e => e.Name),
            ItemImageBinding = Binding.Property<PluginInfo, Image>(static e => e.Icon),
        };

        _listBoxPluginList.SelectedValueChanged += OnSelectedPlugin;

        _gridViewInfo = new GridView
        {
            DataStore = _kvInfo,
            GridLines = GridLines.Both,
            AllowMultipleSelection = false,
            Columns =
            {
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<KVPair, string>(static e => e.Key) },
                    HeaderText = "Name",
                    AutoSize = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<KVPair, string>(static e => e.Value) },
                    HeaderText = "Value",
                    AutoSize = true
                }
            }
        };

        _gridViewObjects = new GridView
        {
            DataStore = _kvObjects,
            GridLines = GridLines.Both,
            AllowMultipleSelection = false,
            Columns =
            {
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<KVPair, string>(static e => e.Key) },
                    HeaderText = "Display Name",
                    AutoSize = true,
                    Sortable = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<KVPair, string>(static e => e.Value) },
                    HeaderText = "Full Name",
                    AutoSize = true,
                    Sortable = true
                }
            }
        };

        _gridViewFiles = new GridView
        {
            DataStore = _kvFiles,
            GridLines = GridLines.Both,
            AllowMultipleSelection = false,
            Columns =
            {
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<KVPair, string>(static e => e.Key) },
                    HeaderText = "Type",
                    AutoSize = true,
                    Sortable = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<KVPair, string>(static e => e.Value) },
                    HeaderText = "Location",
                    AutoSize = true,
                    Sortable = true
                }
            }
        };

        var tabs = new TabControl();
        tabs.Pages.Add(new TabPage(_gridViewInfo, new Padding(10)) { Text = "Information" });
        tabs.Pages.Add(new TabPage(_gridViewObjects, new Padding(10)) { Text = "Objects" });
        tabs.Pages.Add(new TabPage(_gridViewFiles, new Padding(10)) { Text = "Files" });

        Content = new Splitter
        {
            Orientation = Orientation.Horizontal,
            Panel1MinimumSize = 300,
            Panel2MinimumSize = 300,
            Panel1 = new TableLayout
            {
                Spacing = new Size(5, 5),
                Padding = new Padding(10),
                Rows = { new TableRow(_textBoxSearch), new TableRow(_listBoxPluginList) }
            },

            Panel2 = new TableLayout
            {
                Spacing = new Size(5, 5),
                Padding = new Padding(10),
                Rows = { new TableRow(tabs) }
            }
        };
    }

    private void OnSelectedPlugin(object sender, EventArgs e)
    {
        if (_listBoxPluginList.SelectedValue is not PluginInfo info) return;
        if (!_infos.TryGetValue(info.Name, out var detailedInfo)) return;

        _kvObjects.Clear();
        _kvInfo.Clear();
        _kvFiles.Clear();

        foreach (var it in detailedInfo.Infos)
            _kvInfo.Add(it);

        foreach (var it in detailedInfo.Objects)
            _kvObjects.Add(it);

        foreach (var it in detailedInfo.Files)
            _kvFiles.Add(it);
    }
}
