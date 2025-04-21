using Eto.Drawing;
using Eto.Forms;
using PancakeNextCore.GH;
using PancakeNextCore.GH.Upgrader;
using Rhino.PlugIns;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.UI.EtoForms;
internal sealed partial class InternalSettingsUi : Form
{
    private sealed class SettingEntry(ISettingAccessor accessor)
    {
        private readonly ISettingAccessor _setting = accessor;

        public string Name => _setting.InConfigName;
        public string PropertyName => _setting.PropertyName == Name ? "" : _setting.PropertyName;
        public string CurrentValue => _setting.GetString();
        public string DefaultValue => _setting.GetDefaultString();
    }

    private readonly ObservableCollection<SettingEntry> _settings = new();

    private GridView _gridViewSettings;
    public InternalSettingsUi()
    {
        InitializeData();
        InitializeComponents();
    }

    private void InitializeData()
    {
        foreach (var setting in InternalSettingProber.FindSettings())
        {
            _settings.Add(new(setting));
        }
    }
    private void InitializeComponents()
    {
        ClientSize = new Size(1200, 600);
        Title = "about:config";

        /*_textBoxSearch = new SearchBox();

        _textBoxSearch.TextChanged += OnSearchBoxTextChanged;*/

        _gridViewSettings = new GridView
        {
            DataStore = _settings,
            GridLines = GridLines.Both,
            AllowMultipleSelection = false,
            Columns =
            {
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<SettingEntry, string>(static e => e.Name) },
                    HeaderText = "Name",
                    AutoSize = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<SettingEntry, string>(static e => e.PropertyName) },
                    HeaderText = "Member Name (if different)",
                    AutoSize = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<SettingEntry, string>(static e => e.CurrentValue) },
                    HeaderText = "Current Value",
                    AutoSize = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<SettingEntry, string>(static e => e.DefaultValue) },
                    HeaderText = "Default Value",
                    AutoSize = true
                }
            }
        };

        Padding = new(10);
        Content = _gridViewSettings;
    }
}
