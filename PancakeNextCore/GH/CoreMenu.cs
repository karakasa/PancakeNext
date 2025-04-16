using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Runtime;
using System.Text;
using Eto.Forms;
using PancakeNextCore.Dataset;
using Grasshopper2.UI;
using System.Diagnostics.CodeAnalysis;

namespace PancakeNextCore.GH;

internal class MenuConstructor
{
    internal static readonly bool HideLabels = Config.IsRunningOnMac;

    // private static readonly Bitmap Placeholder = Config.IsRunningOnMac ? null : new Bitmap(24, 24);

    protected MenuItemCollection _parent;
    protected bool lastSeparator = false;

    internal MenuConstructor(MenuItemCollection parent)
    {
        _parent = parent;
    }

    internal MenuItem AddFeatureEntry(string displayName, string featureName, bool disallowShortcut = false, string toolTip = null)
    {
        throw new NotImplementedException();
        return null!;

        /*var name = featureName;
        var check = Config.Features.IsEffective(featureName);

        var item = new MenuItem
        {
            Text = displayName,
            Checked = check
        };

        if (toolTip != null)
            item.ToolTipText = toolTip;

        item.Click += (sender, e) =>
        {
            item.Checked = !item.Checked;
            Config.Features.SetStatus(name, item.Checked);
        };
        
        lastSeparator = false;
        _parent.Add(item);

        if (!disallowShortcut)
            CoreMenu.MenuEntryAllowShortcut.Add(item);

        return item;*/
    }

    internal ButtonMenuItem AddEntry(string name, Action procedure, string? toolTip = null)
    {
        ButtonMenuItem item;

        item = new ButtonMenuItem
        {
            Text = name
        };

        item.Click += (_, _) => procedure();

        PolishMenuItem(item, false, toolTip);

        return item;
    }

    internal CheckMenuItem AddToggleEntry(string name, Func<bool, bool> procedure, bool check, string? toolTip = null)
    {
        CheckMenuItem item;

        item = new CheckMenuItem
        {
            Text = name,
            Checked = check
        };

        item.Click += (_, _) =>
        {
            item.Checked = procedure(!item.Checked);
        };

        PolishMenuItem(item, false, toolTip);

        return item;
    }

    internal CheckMenuItem AddToggleEntry(string name, Func<bool> getter, Func<bool, bool> setter, string? toolTip = null)
    {
        return AddToggleEntry(name, setter, getter(), toolTip);
    }

    internal CheckMenuItem AddToggleEntry(string name, Func<bool> getter, Action<bool> setter, string? toolTip = null)
    {
        return AddToggleEntry(name, x =>
        {
            setter(x);
            return x;
        }, getter(), toolTip);
    }

    private void PolishMenuItem(MenuItem item, bool disallowShortcut, string? toolTip)
    {
        if (toolTip != null)
            item.ToolTip = toolTip;


        lastSeparator = false;
        _parent.Add(item);

        if (!disallowShortcut)
            CoreMenu.MenuEntryAllowShortcut.Add(item);
    }

    internal ButtonMenuItem? AddLabel(string name, string sectionName)
    {
        if (HideLabels)
            return null;

        var item = new ButtonMenuItem
        {
            Text = $"[ {name} ]",
            Tag = sectionName,
            Enabled = false
        };

        lastSeparator = false;
        _parent.Add(item);
        return item;
    }

    internal MenuConstructorLazy AddDropdownEntry(string name, out SubMenuItem outItem)
    {
        var item = new SubMenuItem
        {
            Text = name
        };

        lastSeparator = false;
        outItem = item;
        return new MenuConstructorLazy(item.Items, _parent, item);
    }

    internal void AddSeparator()
    {
        if (!lastSeparator)
        {
            _parent.Add(new SeparatorMenuItem());
            lastSeparator = true;
        }
    }
}

internal class MenuConstructorLazy : MenuConstructor, IDisposable
{
    private MenuItemCollection _grandparent;
    private MenuItem _lazyitem;

    internal MenuConstructorLazy(MenuItemCollection parent, MenuItemCollection grandparent,
        MenuItem item) : base(parent)
    {
        _grandparent = grandparent;
        _lazyitem = item;
    }

    public void Dispose()
    {
        if (_parent.Count != 0)
            _grandparent.Add(_lazyitem);
    }
}

internal sealed class CoreMenu
{
    public static readonly CoreMenu Instance = new();

    private readonly SubMenuItem _topMenu = new();

    private const string MenuTitleSafe = "Pancake [Safemode]";
    private const string MenuTitle = "Pancake";

    internal static List<MenuItem> MenuEntryAllowShortcut = new List<MenuItem>();

    internal static void RegisterEntriesForShortcut()
    {
        // TODO
    }

    internal void RegisterMenu()
    {
        if (Editor.Instance is null)
        {
            IssueTracker.ReportInPlace("INIT_CANVAS_FAIL");
            return;
        }

        var GhMenu = Editor.Instance.Menu;

        if (!GhMenu.Items.OfType<MenuItem>().Any(it => it.Text == MenuTitle || it.Text == MenuTitleSafe))
        {
            _topMenu.Items.Clear();
            AddMenuFeatures(_topMenu.Items);
            // ExtensionManager.TriggerDefaultEvent("OnMainMenuCreated", _topMenu.DropDownItems);
            RegisterEntriesForShortcut();

            _topMenu.Text = Config.SafeMode ? MenuTitleSafe : MenuTitle;

            GhMenu.Items.Insert(Math.Min(6, GhMenu.Items.Count), _topMenu);
        }
        else
        {
            IssueTracker.ReportInPlace("Pancake's menu already exists when Pancake loads, probably as a result of being loaded twice. Pancake may be in an unstable state.");
            return;
        }
    }

    private void AddMenuFeatures(MenuItemCollection retMenu)
    {
        var menu = new MenuConstructor(retMenu);

        menu.AddLabel(Strings.CoreMenu_AddMenuFeatures_PrepareForExchange, "exchange");

        menu.AddEntry(Strings.CoreMenu_AddMenuFeatures_InternalizeReferencedGeometry, mnuInternalize_Click, toolTip: Strings.ThisFeatureWillInternalizeAllReferencedGeometryInYourScriptAsIfYouClickInternalizeInAllOfTheMenus);
        menu.AddEntry(Strings.CoreMenu_AddMenuFeatures_CheckPortabilityOfThisDocument, mnuShowDependency_Click, toolTip: Strings.StudysWhatExternalResourcesTheCurrentScriptRequiresAndShowAReport);

        menu.AddSeparator();

        menu.AddLabel(Strings.CoreMenu_AddMenuFeatures_Tweaks, "tweaks");
        //_mnuHangProtection = menu.AddEntry(Strings.CoreMenu_AddMenuFeatures_EnableHangProtection,
            //mnuHangProtection_Click, HangDetector.Enabled, disallowShortcut: true, toolTip: Strings.PancakeWillDoAnEmergencySaveIf);
        //_mnuExtendedMenu = menu.AddEntry(Strings.EnableExtendedContextMenu,
                //mnuExtendedMenu_Click, Config.Features.IsEffective(ExtendedContextMenu.Name), disallowShortcut: true, toolTip: Strings.AddsAContextMenuWhenYouRightClickCertain);

        menu.AddSeparator();

        menu.AddLabel(Strings.CoreMenu_AddMenuFeatures_Utilities, "utility");

        using (var dropdown = menu.AddDropdownEntry(Strings.CoreMenu_AddMenuFeatures_SelectUpDownStreamOrBoth, out _))
        {
            dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_Upstream, mnuPickUpstream_Click, toolTip: Strings.SelectAllComponentsThatAffectTheSelectedComponent);
            dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_Downstream, mnuPickDownstream_Click, toolTip: Strings.SelectAllComponentsThatAreAffectedByTheSelectedComponent);
            dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_UpAndDown, mnuPickAll_Click, toolTip: Strings.SelectAllComponentsThatAffectOrAreAffectedByTheSelectedComponent);

            dropdown.AddSeparator();

            dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_SourceParams, mnuFarEndUpstream_Click, toolTip: Strings.SelectAllIndependentInputsThatAffectTheSelectedComponent);
            dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_DestinationParams, mnuFarEndDownstream_Click, toolTip: Strings.SelectAllIndependentOutputsThatAffectTheSelectedComponent);

            dropdown.AddSeparator();

            //_mnuIncludeComponent = dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_IncludeComponentsInParamSerach,
              //  mnuIncludeComponent_Click, PickStream.IncludeComponent, disallowShortcut: true, toolTip: Strings.ControlIfComponentsContainingPersistentDataAreRegardedAsInpdendentInOutputs);
        }

        /*using (var dropdown = menu.AddDropdownEntry(Strings.AnalyzePerformance, out _))
        {
            dropdown.AddEntry(Strings.EntireDocument, (sender, e) => Performance.ShowEntireDocument());
            dropdown.AddEntry(Strings.Selected, (sender, e) => Performance.ShowSelected());

            dropdown.AddSeparator();

            dropdown.AddEntry(Strings.BenchmarkSelectedComponents, (sender, e) => Performance.BenchmarkSelected());
        }*/

        var versionString = PancakeInfo.CoreVersion.ToString(4);

        menu.AddSeparator();

        menu.AddLabel(string.Format(Strings.CoreMenu_AddMenuFeatures_Pancake_for_GH__0_, versionString), "");

        if (Config.SafeMode || Config.DevMode)
        {
            menu.AddSeparator();
            using var dropdown = menu.AddDropdownEntry(Strings.CoreMenu_AddMenuFeatures_DeveloperTools, out _);
            dropdown.AddToggleEntry(Strings.CoreMenu_AddMenuFeatures_EnableDeveloperMode, () => Config.DevMode, x => Config.DevMode = x);
            dropdown.AddSeparator();
        }
    }

    private void mnuFarEndDownstream_Click()
    {
        throw new NotImplementedException();
    }

    private void mnuFarEndUpstream_Click()
    {
        throw new NotImplementedException();
    }

    private void mnuPickAll_Click()
    {
        throw new NotImplementedException();
    }

    private void mnuPickUpstream_Click()
    {
        throw new NotImplementedException();
    }

    private void mnuPickDownstream_Click()
    {
        throw new NotImplementedException();
    }

    private void mnuShowDependency_Click()
    {
        throw new NotImplementedException();
    }

    private void mnuInternalize_Click()
    {
        throw new NotImplementedException();
    }
}
