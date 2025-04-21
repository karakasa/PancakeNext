using Eto.Drawing;
using Eto.Forms;
using PancakeNextCore.GH;
using PancakeNextCore.GH.Upgrader;
using PancakeNextCore.Utility;
using Rhino.PlugIns;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.UI.EtoForms;
internal sealed partial class FormInternalSettings : Form
{
    private sealed class SettingEntry(ISettingAccessor accessor) : INotifyPropertyChanged
    {
        public ISettingAccessor Accessor { get; } = accessor;

        public string Name => Accessor.InConfigName;
        public string PropertyName => Accessor.PropertyName == Name ? "" : Accessor.PropertyName;
        public string CurrentValue
        {
            get => Accessor.GetString();
            set
            {
                if (value == Accessor.GetString()) return;

                if (value == "/default") value = DefaultValue;

                if (Accessor.SetByString(value))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentValue)));
                }
            }
        }
        public string DefaultValue => Accessor.GetDefaultString();
        public Delegate[]? Listeners { get; private set; }
        public string ListenersDescription
        {
            get
            {
                Listeners ??= Accessor.GetInvocationList();
                if (Listeners.Length == 0) return "";
                return Listeners.Length.ToString();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private readonly ObservableCollection<SettingEntry> _settings = new();

    private GridView _gridViewSettings;
    public FormInternalSettings()
    {
        InitializeData();
        InitializeComponents();
    }

    private void InitializeData()
    {
        foreach (var setting in InternalSettingProber.FindSettings().OrderBy(s => s.InConfigName).ThenBy(s => s.PropertyName))
        {
            _settings.Add(new(setting));
        }
    }

    [MemberNotNull(nameof(_gridViewSettings))]
    private void InitializeComponents()
    {
        ClientSize = new Size(1200, 600);
        Title = "about:config";

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
                    DataCell = new TextBoxCell(nameof(SettingEntry.CurrentValue)){TextAlignment = TextAlignment.Right},
                    HeaderText = "Current Value",
                    AutoSize = true,
                    Editable = true,
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<SettingEntry, string>(static e => e.DefaultValue), TextAlignment = TextAlignment.Right },
                    HeaderText = "Default Value",
                    AutoSize = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell { Binding = Binding.Property<SettingEntry, string>(static e => e.ListenersDescription), TextAlignment = TextAlignment.Right },
                    HeaderText = "Listener",
                    AutoSize = true
                }
            }
        };

        Padding = new(10);
        Content = _gridViewSettings;

        _gridViewSettings.CellDoubleClick += (sender, e) =>
        {
            if (e.Column == 4)
            {
                if (_gridViewSettings.SelectedItem is not SettingEntry entry) return;
                var delegates = entry.Listeners;
                if (delegates is null || delegates.Length == 0) return;
                using var form = new FormListeners(entry.Name, delegates);
                form.ShowModal(this);
            }
        };
    }
}

internal sealed partial class FormListeners : Dialog
{
    private sealed class DelegateEntry(string method, string site)
    {
        public string Method { get; } = method;
        public string Site { get; } = site;
    }

    private readonly ObservableCollection<DelegateEntry> _entries = [];

    private GridView _gridViewSettings;
    public FormListeners(string title, Delegate[] delegates)
    {
        Title = $"Listeners of {title}";

        InitializeData(delegates);
        InitializeComponents();
    }

    private void InitializeData(Delegate[] delegates)
    {
        foreach (var d in delegates)
        {
            _entries.Add(new(d.Method?.ToString() ?? "", d.Target?.ToString() ?? ""));
        }
    }

    [MemberNotNull(nameof(_gridViewSettings))]
    private void InitializeComponents()
    {
        ClientSize = new Size(600, 200);

        _gridViewSettings = new GridView
        {
            DataStore = _entries,
            GridLines = GridLines.Both,
            AllowMultipleSelection = false,
            Columns =
            {
                new GridColumn
                {
                    DataCell = new TextBoxCell(nameof(DelegateEntry.Method)),
                    HeaderText = "Method",
                    AutoSize = true
                },
                new GridColumn
                {
                    DataCell = new TextBoxCell(nameof(DelegateEntry.Site)),
                    HeaderText = "Target",
                    AutoSize = true
                }
            }
        };

        Padding = new(10);
        Content = _gridViewSettings;
    }
}
