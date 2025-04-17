using Eto.Forms;
using PancakeNextCore.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.UI.EtoForms;

internal partial class FormTransferSetting : Dialog
{
    public Presenter.TransferWindowResult Result { get; private set; } = Presenter.TransferWindowResult.Cancelled;

    private GridView<SelectableTransferSettingModule> _listCheckBoxes;
    private Button _buttonLoadFrom;
    private Button _buttonSaveTo;
    public FormTransferSetting()
    {
        InitializeComponents();
    }

    private readonly ObservableCollection<SelectableTransferSettingModule> _settings = new();

    private class SelectableTransferSettingModule
    {
        public Modules.TransferSetting.Base ParentObject { get; private set; }
        public string Name { get; private set; }
        public bool? Selected { get; set; }
        public SelectableTransferSettingModule(Modules.TransferSetting.Base setting)
        {
            ParentObject = setting;
            Name = setting.FriendlyName;
            Selected = setting.EnabledByDefault;
        }

        public override string ToString()
            => Name;
    }
    public IEnumerable<Modules.TransferSetting.Base> GetSelectedSettings()
    {
        return _settings
            .Where(s => s.Selected ?? false)
            .Select(s => s.ParentObject);
    }
    public void AddSettings(IEnumerable<Modules.TransferSetting.Base> settings)
    {
        foreach (var setting in settings)
            _settings.Add(new SelectableTransferSettingModule(setting));
    }
    private void InitializeComponents()
    {
        Title = "Transfer settings";
        ClientSize = new Eto.Drawing.Size(400, 150);

        _buttonLoadFrom = new Button
        {
            Height = 65,
            Text = "Load from..."
        };

        _buttonSaveTo = new Button
        {
            Height = 65,
            Text = "Save to..."
        };

        _buttonLoadFrom.Click += OnLoadFrom;
        _buttonSaveTo.Click += OnSaveTo;

        _listCheckBoxes = new()
        {
            Width = 250,
            DataStore = _settings,
            Columns = {
                new GridColumn
                {
                    HeaderText = "",
                    AutoSize = true,
                    DataCell = new CheckBoxCell
                    {
                        Binding = Binding.Property<bool?>(nameof(SelectableTransferSettingModule.Selected))
                    },
                    Editable = true
                },
                new GridColumn
                {
                    HeaderText = "Section",
                    AutoSize = true,
                    DataCell = new TextBoxCell{
                        Binding = Binding.Property<string>(nameof(SelectableTransferSettingModule.Name))
                    }
                }
            }
        };

        Content = new TableLayout
        {
            Spacing = new Eto.Drawing.Size(5, 5),
            Padding = new Eto.Drawing.Padding(10),
            Rows = {new TableRow(
                _listCheckBoxes,
                new TableLayout
                {
                    Rows =
                    {
                        new TableRow(_buttonLoadFrom),
                        new TableRow(_buttonSaveTo)
                    }
                }
                ) }
        };
    }

    private void OnLoadFrom(object sender, EventArgs e)
    {
        Result = Presenter.TransferWindowResult.LoadFromFile;
        Close();
    }

    private void OnSaveTo(object sender, EventArgs e)
    {
        Result = Presenter.TransferWindowResult.SaveToFile;
        Close();
    }
}
