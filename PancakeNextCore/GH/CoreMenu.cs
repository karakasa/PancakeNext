using Eto.Forms;
using Grasshopper2.UI;
using PancakeNextCore.Dataset;
using PancakeNextCore.Helper;
using PancakeNextCore.UI;
using PancakeNextCore.UI.EtoForms;
using PancakeNextCore.Utility;
using Rhino.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;

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

    internal CheckMenuItem AddFeatureEntry(string displayName, string featureName, bool disallowShortcut = false, string? toolTip = null)
    {
        var name = featureName;
        var check = PluginLifetime.Features.IsEffective(featureName);

        return AddToggleEntry(displayName, check,
            expectedState =>
            {
                PluginLifetime.Features.SetStatus(name, expectedState);
                return PluginLifetime.Features.IsEffective(name);
            }, 
            toolTip);
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

    internal CheckMenuItem AddToggleEntry(string name, bool check, Func<bool, bool> procedure, string? toolTip = null)
    {
        CheckMenuItem item;

        item = new CheckMenuItem
        {
            Text = name,
            Checked = check
        };

        item.Click += (_, _) =>
        {
            item.Checked = procedure(item.Checked);
        };

        PolishMenuItem(item, false, toolTip);

        return item;
    }

    internal CheckMenuItem AddToggleEntry(string name, Func<bool> getter, Func<bool, bool> setter, string? toolTip = null)
    {
        return AddToggleEntry(name, getter(), setter, toolTip);
    }

    internal CheckMenuItem AddToggleEntry(string name, Func<bool> getter, Action<bool> setter, string? toolTip = null)
    {
        return AddToggleEntry(name, getter(), x =>
        {
            setter(x);
            return x;
        }, toolTip);
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
        menu.AddFeatureEntry(Strings.EnableExtendedContextMenu, ExtendedContextMenu.Name, toolTip: Strings.AddsAContextMenuWhenYouRightClickCertain);
        menu.AddFeatureEntry(Strings.EnableParamAccessOverlay, OverlayInfo.Name);
        menu.AddFeatureEntry(Strings.ParamContentHint, ParamContentHint.Name);

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

        using (var dropdown = menu.AddDropdownEntry(Strings.AnalyzePerformance, out _))
        {
            dropdown.AddEntry(Strings.EntireDocument, Performance.ShowEntireDocument);
            dropdown.AddEntry(Strings.Selected, Performance.ShowSelected);

            dropdown.AddSeparator();

            dropdown.AddEntry(Strings.BenchmarkSelectedComponents, Performance.BenchmarkSelected);
        }

        var versionString = PancakeInfo.CoreVersion.ToString(4);

        menu.AddSeparator();

        menu.AddLabel(string.Format(Strings.CoreMenu_AddMenuFeatures_Pancake_for_GH__0_, versionString), "");

        if (Config.SafeMode || Config.DevMode || Config.PreRelease)
        {
            menu.AddSeparator();
            using var dropdown = menu.AddDropdownEntry(Strings.CoreMenu_AddMenuFeatures_DeveloperTools, out _);
            dropdown.AddToggleEntry(Strings.CoreMenu_AddMenuFeatures_EnableDeveloperMode, () => Config.DevMode, x => Config.DevMode = x);
            dropdown.AddSeparator();
            dropdown.AddEntry("List core components...", mnuListCoreComponents);
            dropdown.AddEntry("G2 Internal settings...", mnuListInternalSettings);
            dropdown.AddSeparator();
            dropdown.AddEntry("Enable canvas debug overlay", mnuEnableDebugOverlay);
            dropdown.AddEntry("Benchmark canvas performance", mnuBenchmarkCanvas);
#if DEBUG
            dropdown.AddSeparator();
            dropdown.AddEntry("Force clear compiled icons", () => Utility.PathBasedIcon.CompiledPathIcon.DestroyAllCaches());
#endif
        }
    }

    private void mnuBenchmarkCanvas()
    {
        CanvasDebug.BenchmarkFps();
    }

    private void mnuListInternalSettings()
    {
        Presenter.ShowWindow<FormInternalSettings>();
    }

    private void mnuEnableDebugOverlay()
    {
        var canvas = Editor.Instance.Canvas;
        ReflectionHelper.SetProperty(canvas, "ShowDebugOverlay", true, false);
    }

    private static void mnuListCoreComponents()
    {
        var sb = new StringBuilder();
        foreach (var proxy in DbgInfo.GetCoreComponents())
        {
            sb.AppendLine($"{proxy.Id},{proxy.Nomen.Name},{proxy.Nomen.Chapter},{proxy.Nomen.Section}");
        }

        Presenter.ShowReportWindow(sb.ToString());
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
