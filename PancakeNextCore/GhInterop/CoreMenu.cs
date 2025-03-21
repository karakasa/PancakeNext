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

namespace PancakeNextCore.GhInterop;

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
    internal MenuItem AddEntry(string name, EventHandler<EventArgs> procedure, bool? check = null, bool disallowShortcut = false, string toolTip = null)
    {
        MenuItem item;

        if (check is null)
        {
            item = new ButtonMenuItem
            {
                Text = name
            };
        }
        else
        {
            item = new CheckMenuItem
            {
                Text = name,
                Checked = check ?? false
            };
        }

        /* if (check == null && !Config.IsMac)
            item.Image = Placeholder; */

        if (toolTip != null)
            item.ToolTip = toolTip;

        if (procedure != null)
            item.Click += procedure;

        lastSeparator = false;
        _parent.Add(item);

        if(!disallowShortcut)
            CoreMenu.MenuEntryAllowShortcut.Add(item);

        return item;
    }

    internal ButtonMenuItem? AddLabel(string name, string sectionName)
    {
        if (HideLabels)
            return null;

        var item = new ButtonMenuItem
        {
            Text = name,
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

        // if (!Config.IsRunningOnMac)
            // item.Image = Placeholder;

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
        if(_parent.Count != 0)
            _grandparent.Add(_lazyitem);
    }
}

internal sealed class CoreMenu
{
    private SubMenuItem _topMenu = new();
    private MenuItem _mnuEnableVersionWaterstamp;
    private MenuItem _mnuEnableVersionWaterstampRhino;
    private MenuItem _mnuHangProtection;
    private MenuItem _mnuUnblockMonitor;
    private MenuItem _mnuIncludeComponent;
    private MenuItem _mnuCheckDownload;
    private MenuItem _mnuClusterSkipRef;
    private MenuItem _mnuDevMode;
    private MenuItem _mnuDisableComponent;
    private MenuItem _mnuOverlayInfo;
    private MenuItem _mnuParamContentOverlay;
    private ButtonMenuItem _mnuTitleExchange;
    private ButtonMenuItem _mnuTitleVersion;
    private ButtonMenuItem _mnuTitleTweaks;
    private ButtonMenuItem _mnuTitleUtilities;
    private ButtonMenuItem _mnuTitleAbout1;

    private const string MenuTitleSafe = "Pancake [Safemode]";
    private const string MenuTitle = "Pancake";

    internal static List<MenuItem> MenuEntryAllowShortcut = new List<MenuItem>();
    private MenuItem _mnuExtendedMenu;
    private MenuItem _mnuEnableLog;
    private MenuItem _mnuForceIdlingHang;
    private MenuItem _mnuEnableBetaFeatures;

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
        /*var menu = new MenuConstructor(retMenu);

        _mnuTitleExchange = menu.AddLabel("Strings.CoreMenu_AddMenuFeatures_PrepareForExchange", "exchange");

        menu.AddEntry(Strings.CoreMenu_AddMenuFeatures_InternalizeReferencedGeometry, mnuInternalize_Click, toolTip: Strings.ThisFeatureWillInternalizeAllReferencedGeometryInYourScriptAsIfYouClickInternalizeInAllOfTheMenus);
        menu.AddEntry(Strings.CoreMenu_AddMenuFeatures_CheckPortabilityOfThisDocument, mnuShowDependency_Click, disallowShortcut: true, toolTip: Strings.StudysWhatExternalResourcesTheCurrentScriptRequiresAndShowAReport);

        menu.AddSeparator();

        _mnuTitleTweaks = menu.AddLabel(Strings.CoreMenu_AddMenuFeatures_Tweaks, "tweaks");
        _mnuHangProtection = menu.AddEntry(Strings.CoreMenu_AddMenuFeatures_EnableHangProtection,
            mnuHangProtection_Click, HangDetector.Enabled, disallowShortcut: true, toolTip: Strings.PancakeWillDoAnEmergencySaveIf);
        _mnuExtendedMenu = menu.AddEntry(Strings.EnableExtendedContextMenu,
                mnuExtendedMenu_Click, Config.Features.IsEffective(ExtendedContextMenu.Name), disallowShortcut: true, toolTip: Strings.AddsAContextMenuWhenYouRightClickCertain);

        menu.AddSeparator();

        _mnuTitleUtilities = menu.AddLabel(Strings.CoreMenu_AddMenuFeatures_Utilities, "utility");

        using (var dropdown = menu.AddDropdownEntry(Strings.CoreMenu_AddMenuFeatures_SelectUpDownStreamOrBoth, out _))
        {
            dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_Upstream, mnuPickUpstream_Click, toolTip: Strings.SelectAllComponentsThatAffectTheSelectedComponent);
            dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_Downstream, mnuPickDownstream_Click, toolTip: Strings.SelectAllComponentsThatAreAffectedByTheSelectedComponent);
            dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_UpAndDown, mnuPickAll_Click, toolTip: Strings.SelectAllComponentsThatAffectOrAreAffectedByTheSelectedComponent);

            dropdown.AddSeparator();

            dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_SourceParams, mnuFarEndUpstream_Click, toolTip: Strings.SelectAllIndependentInputsThatAffectTheSelectedComponent);
            dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_DestinationParams, mnuFarEndDownstream_Click, toolTip: Strings.SelectAllIndependentOutputsThatAffectTheSelectedComponent);

            dropdown.AddSeparator();

            _mnuIncludeComponent = dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_IncludeComponentsInParamSerach,
                mnuIncludeComponent_Click, PickStream.IncludeComponent, disallowShortcut: true, toolTip: Strings.ControlIfComponentsContainingPersistentDataAreRegardedAsInpdendentInOutputs);
        }

        using (var dropdown = menu.AddDropdownEntry(Strings.AnalyzePerformance, out _))
        {
            dropdown.AddEntry(Strings.EntireDocument, (sender, e) => Performance.ShowEntireDocument());
            dropdown.AddEntry(Strings.Selected, (sender, e) => Performance.ShowSelected());

            dropdown.AddSeparator();

            dropdown.AddEntry(Strings.BenchmarkSelectedComponents, (sender, e) => Performance.BenchmarkSelected());
        }

        var versionString = Updater.CoreVersion.ToString(4);

        menu.AddSeparator();

        _mnuTitleAbout1 = menu.AddLabel(string.Format(Strings.CoreMenu_AddMenuFeatures_Pancake_for_GH__0_, versionString), "");

        if (Config.SafeMode || Config.DevMode)
        {
            menu.AddSeparator();
            using (var dropdown = menu.AddDropdownEntry(Strings.CoreMenu_AddMenuFeatures_DeveloperTools, out _))
            {
                _mnuDevMode = dropdown.AddEntry(Strings.CoreMenu_AddMenuFeatures_EnableDeveloperMode, mnuDevMode_Click, Config.DevMode);
                dropdown.AddSeparator();
            }
        }*/
    }
}
