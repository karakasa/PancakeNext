using Eto.Forms;
using Grasshopper;
using Pancake.Helper;
using Pancake.Modules.PortabilityChecker;
using Pancake.Modules.PortabilityCheckerActions;
using Pancake.Utility;
using PancakeNextCore.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pancake.UI.EtoForms;

internal partial class FormPortabilityReport : IFormPortabilityReport
{
    static FormPortabilityReport()
    {
        LoadActions();
    }

    private static IPortabilityCheckerAction[] _actions;
    private static void LoadActions()
    {
        _actions = ReflectionHelper
            .GetEnumerableOfType<IPortabilityCheckerAction>()
            .OrderBy(a => a.Order)
            .ToArray();
    }
    private string[] Configurations;
    public IEnumerable<Guid> SelectedObjects => throw new NotImplementedException();
    private void InitializeConfigurations()
    {
        Configurations = PortabilityReport.AllConfigurations.ToArray();
        Array.Sort(Configurations);
        var defName = PortabilityReport.GetDefaultConfiguration(Instances.ActiveCanvas?.Document);

        _dropDownTarget.DataStore = Configurations;
        _dropDownTarget.SelectedValue = defName;
    }

    public ResultEntry[] Results { get; private set; }

    private TreeGridItem AddTreeNode(string key, string name)
    {
        var giItem = TreeNode(key, name);
        _treeItems.Add(giItem);

        return giItem;
    }
    private TreeGridItem TreeNode(string key, string name)
    {
        var giItem = new TreeGridItem();
        giItem.SetValue(0, name);
        giItem.Tag = key;
        giItem.Expanded = true;

        return giItem;
    }
    public void RefreshContent()
    {
        var name = _dropDownTarget.SelectedValue as string;
        
        if (string.IsNullOrEmpty(name)) return;
        if (!PortabilityReport.TryGetConfiguration(name, out var config)) return;

        var doc = Instances.ActiveCanvas?.Document;

        Results = config.Checkers.SelectMany(chkr => chkr.AnalyzeDocument(doc)).ToArray();

        // BUG : will crash Rhino.

        _treeView.SuspendLayout();

        _treeItems.Clear();

        var nodeCache = new Dictionary<string, TreeGridItem>();

        for (var i = 0; i < Results.Length; i++)
        {
            var result = Results[i];

            if (!nodeCache.TryGetValue(result.Section, out var node))
            {
                nodeCache[result.Section] = node = AddTreeNode("", result.Section);
            }

            if (string.IsNullOrEmpty(result.Name)) continue;

            var secondName = $"{result.Section}/{result.Name}";
            if (nodeCache.TryGetValue(secondName, out var subNode))
            {
                if (!string.IsNullOrEmpty(subNode.Tag as string))
                {
                    string subName = null;
                    subName = Results[Convert.ToInt32(subNode.Tag as string)].SubNameOverride ?? (subNode.GetValue(0) as string);

                    subNode.Children.Add(TreeNode(subNode.Tag as string, subName));
                    subNode.Tag = string.Empty;
                }

                subNode.Children.Add(TreeNode(i.ToString(), result.SubNameOverride ?? result.Name));
            }
            else
            {
                nodeCache[secondName] = subNode = TreeNode(i.ToString(), result.Name);
                node.Children.Add(subNode);
                if (result.AlwaysExpand)
                {
                    subNode.Tag = "";
                    subNode.Children.Add(TreeNode(i.ToString(), result.SubNameOverride));
                }
            }
        }

        if (_treeItems.Count > 0)
        {
            SortTreeNode(_treeItems);
            _treeView.ReloadData();
            _treeView.ScrollToRow(0);
        }
        else
        {
            AddTreeNode("", Strings.NoIssuesAreDetected);
            _treeView.ReloadData();
        }

        for (var i = 0; i < _actions.Length; i++)
        {
            _actionButtons[i].Enabled = _actions[i].ShouldEnable(Results);
        }

        _treeView.ResumeLayout();
    }
    private class TreeViewNodeSorter : IComparer<ITreeGridItem>
    {
        public int Compare(ITreeGridItem x, ITreeGridItem y)
        {
            if (x.Parent == null || y.Parent == null) return 0;
            var str1 = (x as TreeGridItem).GetValue(0) as string;
            var str2 = (y as TreeGridItem).GetValue(0) as string;

            return str1.CompareTo(str2);
        }
    }
    private static readonly TreeViewNodeSorter NodeSorter = new();
    private static void SortTreeNode(TreeGridItemCollection nodes)
    {
        if (nodes is null)
            return;

        nodes.Sort(NodeSorter);

        foreach (var it in nodes)
        {
            if (it.Expandable)
                SortTreeNode((it as TreeGridItem).Children);
        }
    }
}
