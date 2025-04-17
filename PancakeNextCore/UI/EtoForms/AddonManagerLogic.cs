using Eto.Drawing;
using Grasshopper;
using Pancake.Dataset;
using Pancake.GH;
using Pancake.Helper;
using PancakeNextCore.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pancake.UI.EtoForms;

internal partial class FormAddonManager
{
    private class KVPair
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public KVPair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator KVPair(KeyValuePair<string, string> d) => new KVPair(d.Key, d.Value);
    }

    private struct PluginDetailedInfo
    {
        public KVPair[] Infos;
        public KVPair[] Objects;
        public KVPair[] Files;
    }

    private class PluginInfo
    {
        public Image Icon
        {
            get;
            set;
        }
        public string Name { get; set; }
        public int Id { get; set; }

        public PluginInfo(int id, string name, Image icon = null)
        {
            Id = id;
            Name = name;
            Icon = icon;
        }
    }

    private readonly ObservableCollection<PluginInfo> _plugins = new();
    private readonly ObservableCollection<PluginInfo> _pluginsSearchResult = new();
    private readonly ObservableCollection<KVPair> _kvInfo = new();
    private readonly ObservableCollection<KVPair> _kvObjects = new();
    private readonly ObservableCollection<KVPair> _kvFiles = new();

    private readonly List<string> _keyCache = new();
    private readonly List<string> _nameCache = new();

    private readonly List<string> _append1 = new();
    private readonly List<string> _append2 = new();

    private readonly Dictionary<string, PluginDetailedInfo> _infos = new();
    public void AddPlugin(string searchKey, string name, Image icon = null, string append1 = "",
        string append2 = "", int id = -1)
    {
        if (String.IsNullOrWhiteSpace(name))
            name = $"<Unknown> {id}";

        _keyCache.Add(searchKey);
        _nameCache.Add(name);
        _append1.Add(append1);
        _append2.Add(append2);

        _plugins.Add(new PluginInfo(id, name, icon));
    }

    public void AddPluginInfo(string name, IEnumerable<KeyValuePair<string, string>> info, IEnumerable<KeyValuePair<string, string>> file, IEnumerable<KeyValuePair<string, string>> objs, int id = -1)
    {
        if (String.IsNullOrWhiteSpace(name))
            name = $"<Unknown> {id}";
        
        _infos[name] = new PluginDetailedInfo
        {
            Infos = info.Select(e => (KVPair)e).ToArray(),
            Files = file.Select(e => (KVPair)e).OrderBy(kv => kv.Key).ThenBy(kv => kv.Value).ToArray(),
            Objects = objs.Select(e => (KVPair)e).OrderBy(kv => kv.Value).ToArray()
        };
    }
    private static bool MatchString(string pattern, string test)
    {
        var reverse = false;

        if (pattern.Length <= 0)
            return false;

        if (pattern[0] == '!')
        {
            pattern = pattern.Substring(1);
            reverse = true;
        }

        if (pattern.Length <= 0)
            return false;

        var after = pattern.Substring(1);

        switch (pattern[0])
        {
            case '=':
                return test.Equals(after, StringComparison.CurrentCulture) ^ reverse;
            case '[':
                return test.StartsWith(after, StringComparison.CurrentCulture) ^ reverse;
            case ']':
                return test.EndsWith(after, StringComparison.CurrentCulture) ^ reverse;
            case '+':
                return test.Contains(after) ^ reverse;
            default:
                return test.Contains(pattern) ^ reverse;
        }
    }

    private void OnSearchBoxTextChanged(object sender, EventArgs e)
    {
        var text = _textBoxSearch.Text;
        var lower = text.ToLowerInvariant();

        if (string.IsNullOrEmpty(lower))
        {
            _listBoxPluginList.DataStore = _plugins;
            return;
        }

        _pluginsSearchResult.Clear();

        var dbgCmd = false;
#if DEBUG

        {
            var cmd = "";
            var aftercmd = "";
            var index = lower.IndexOf(":", StringComparison.Ordinal);

            if (index != -1)
            {
                cmd = lower.Substring(0, index);
                aftercmd = lower.Substring(index + 1);
            }

            dbgCmd = true;

            switch (cmd)
            {
                case "optimizer":
                    switch (aftercmd)
                    {
                        case "setall":
                            StartupOptimizer.WriteMechanismChangedNames(StartupOptimizer.SetAllLoadMechanism());
                            break;
                        case "unsetall":
                            StartupOptimizer.UnsetAllLoadMechanism(StartupOptimizer.ReadMechanismChangedNames());
                            break;
                    }
                    break;
                case "debug":
                    switch (aftercmd)
                    {
                        case "loadtime":
                            UiHelper.Information(PancakePriority.LoadTime.ToString());
                            break;
                    }
                    break;
                case "hack_remove" when Config.DevMode:
                    DbgInfo.RemoveClusterPassword(Instances.ActiveCanvas?.Document.SelectedObjects().First());
                    break;
                case "guidlist":
                    var str = string.Join("\r\n", Instances.ComponentServer.Libraries.Select(lib => $"{lib.Name} {lib.Id}"));
                    UiHelper.Information(str);
                    _textBoxSearch.Text = string.Empty;
                    break;
                case "testunblockwindow":

                    var listNames = new[] { "AAA", "BBB", "CCC" }.ToList();
                    var listValues = new[] { "1", "2", "3" }.ToList();

                    Presenter.ShowUnblockForm(
                        listNames,
                        listValues,
                        static list => System.Windows.Forms.MessageBox.Show($"ListValues: \r\n{string.Join(Environment.NewLine, list)}"),
                        static boolValue => System.Windows.Forms.MessageBox.Show($"AutoBlock: {boolValue}")
                        );

                    _textBoxSearch.Text = string.Empty;
                    break;
                default:
                    dbgCmd = false;
                    break;
            }
        }
#endif

        if (!dbgCmd)
        {
            for (var i = 0; i < _keyCache.Count; i++)
            {
                var accepted = text == "";
                if (!accepted)
                {
                    var cmd = "";
                    var aftercmd = "";
                    var index = lower.IndexOf(":", StringComparison.Ordinal);

                    if (index != -1)
                    {
                        cmd = lower.Substring(0, index);
                        aftercmd = lower.Substring(index + 1);
                    }
                    switch (cmd)
                    {
                        case "all":
                            accepted = MatchString(aftercmd, _keyCache[i].ToLowerInvariant());
                            break;
                        case "name":
                            accepted = MatchString(aftercmd, _nameCache[i].ToLowerInvariant());
                            break;
                        case "author":
                            accepted = MatchString(aftercmd, _append2[i].ToLowerInvariant());
                            break;
                        case "ver":
                        case "version":
                            accepted = MatchString(aftercmd, _append1[i].ToLowerInvariant());
                            break;
                        case "info":
                            index = aftercmd.IndexOf(":", StringComparison.Ordinal);
                            if (index == -1)
                            {
                                cmd = "";
                                break;
                            }
                            cmd = aftercmd.Substring(0, index);
                            aftercmd = aftercmd.Substring(index + 1);

                            if (cmd == "" || aftercmd == "")
                                break;

                            var info = _infos[_nameCache[i]];
                            if (info.Infos.Any(x =>
                                MatchString(cmd, x.Key.ToLowerInvariant()) &&
                                MatchString(aftercmd, x.Value.ToLowerInvariant())))
                                accepted = true;

                            break;
                        case "obj":
                        case "docobj":
                        case "object":
                            accepted = _infos[_nameCache[i]].Objects.Any(x =>
                                MatchString(aftercmd, x.Key.ToLowerInvariant()) ||
                                MatchString(aftercmd, x.Value.ToLowerInvariant()));
                            break;
                        case "file":
                            accepted = _infos[_nameCache[i]].Files
                                .Any(x => MatchString(aftercmd, x.Value.ToLowerInvariant()));
                            break;
                        case "delayload":
                            var plugin = _infos[_nameCache[i]];
                            accepted = plugin.Files.Length <= 1 &&
                                plugin.Infos.Any(t => t.Key == Strings.LoadingMechanism && t.Value == "COFF") &&
                                plugin.Infos.Any(t => t.Key == Strings.CoreLibrary && t.Value == "false");
                            break;

                        default:
                            cmd = "";
                            break;
                    }

                    if (cmd == "")
                        accepted = MatchString(lower, _keyCache[i].ToLowerInvariant());
                }

                if (!accepted) continue;

                _pluginsSearchResult.Add(_plugins[i]);
            }

            _listBoxPluginList.DataStore = _pluginsSearchResult;
        }
    }
}
