using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Pancake.Dataset;
using Pancake.GH.Tweaks;

namespace PancakeNextCore.Helper;

internal static class HighlightManager
{
    internal static GhHighlightArtist TagArtist => GhHighlightArtist.Instance;

    internal static void RegisterSharedHighlightArtist(GH_Canvas canvas)
    {
        canvas.AddTagArtist(TagArtist);
    }

    static HighlightManager()
    {
        ReadOptions();
    }

    private static void ReadOptions()
    {
        var config = Config.Read("HighlightChanges", "off");
        switch (config)
        {
            case "off":
                _state = State.Off;
                break;
            case "selection":
                _state = State.BySelection;
                break;
            case "group":
                _state = State.ByGrouping;
                break;
            case "tag":
                _state = State.ByTagArtist;
                break;
        }
    }

    private static void WriteOptions()
    {
        switch (_state)
        {
            case State.Off:
                Config.Write("HighlightChanges", "off");
                break;
            case State.BySelection:
                Config.Write("HighlightChanges", "selection");
                break;
            case State.ByGrouping:
                Config.Write("HighlightChanges", "group");
                break;
            case State.ByTagArtist:
                Config.Write("HighlightChanges", "tag");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal const string NamePrefix = "pancake.";
    internal static readonly Color GroupColor = Color.FromArgb(150, 255, 0, 0);
    private static readonly List<GH_Group> HighlightedGroups = new List<GH_Group>();

    public static bool AlwaysRemovePreviousResult = true;

    public enum State
    {
        Undefined,
        Off,
        BySelection,
        ByGrouping,
        ByTagArtist
    }

    private static State _state = State.Undefined;

    public static State CurrentState
    {
        get => _state;
        set => SetCurrentState(value);
    }

    private static void RemoveAllSavedGroups()
    {
        foreach (var it in HighlightedGroups)
        {
            var doc = it.OnPingDocument();
            doc?.RemoveObject(it, false);
        }

        HighlightedGroups.Clear();
    }

    private static void RemoveAllGroupsByName(GH_Document doc)
    {
        if (doc == null)
            return;
        var tobeRemoved = new List<GH_Group>();

        foreach (var it in doc.Objects)
        {
            if (!(it is GH_Group group))
                continue;

            if (group.Description.StartsWith(NamePrefix, StringComparison.Ordinal))
                tobeRemoved.Add(group);
        }

        if (tobeRemoved.Count != 0)
            doc.RemoveObjects(tobeRemoved, false);
    }

    private static void RemoveAllTags()
    {
        TagArtist.RemoveAllHighlights();
    }

    private static void RemoveAllIfPossible()
    {
        RemoveAllSavedGroups();
        RemoveAllTags();
        RemoveAllGroupsByName(Instances.ActiveCanvas.Document);
    }

    public static void RemoveAll()
    {
        RemoveAllIfPossible();
        Instances.RedrawCanvas();
    }

    private static void SetCurrentState(State newState)
    {
        RemoveAllIfPossible();
        _state = newState;
        WriteOptions();
    }

    private static void AddHighlightSelection(IEnumerable<IGH_DocumentObject> objects, string description)
    {
        var doc = Instances.ActiveCanvas.Document;
        if (doc == null)
            return;

        GhGui.SelectObjects(objects);
    }

    private static void AddHighlightTag(IEnumerable<IGH_DocumentObject> objects, string description)
    {
        var doc = Instances.ActiveCanvas.Document;
        if (doc == null)
            return;

        if (AlwaysRemovePreviousResult)
            RemoveAllTags();

        TagArtist.AddHighlight(objects);
    }

    private static void AddHighlightGroup(IEnumerable<IGH_DocumentObject> objects, string description)
    {
        var doc = Instances.ActiveCanvas.Document;
        if (doc == null)
            return;

        if (AlwaysRemovePreviousResult)
            RemoveAllSavedGroups();

        foreach (var it in objects)
        {
            var group = new GH_Group
            {
                NickName = description,
                Description = NamePrefix + description,
                Colour = GroupColor
            };

            group.AddObject(it.InstanceGuid);
            if (doc.AddObject(group, false))
                HighlightedGroups.Add(group);
        }
    }

    public static void AddHighlight(IEnumerable<IGH_DocumentObject> objects, string description)
    {
        switch (_state)
        {
            case State.Off:
                break;
            case State.BySelection:
                AddHighlightSelection(objects, description);
                break;
            case State.ByGrouping:
                AddHighlightGroup(objects, description);
                break;
            case State.ByTagArtist:
                AddHighlightTag(objects, description);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Instances.RedrawCanvas();
    }
}
