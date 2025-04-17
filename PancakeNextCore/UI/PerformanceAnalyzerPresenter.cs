using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Pancake.Dataset;
using Pancake.GH.Tweaks;
using Pancake.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.UI;

public enum SpecialEntryType
{
    EmptyRow,
    MinorComponents,
    Total
}
public class PerformanceAnalyzerEntry
{
    public Guid ObjectId { get; set; }
    public string ObjectName { get; set; }
    public string DisplayTime { get; set; }
    public string DisplayPercentage { get; set; }
    public string DisplayRunCount { get; set; }
    public string DisplayTimePerRun { get; set; }
    public int ImageId { get; set; }

    public const int NO_IMAGE = -1;
}
public enum GroupCriteria
{
    ByComponent,
    ByGroup,
    ByCategory
}
public static class PerformanceAnalyzerPresenter
{
    public static int MinimumElapse = 5;
    private struct IndividualRecord
    {
        public string Name;
        public int IconId;
        public IndividualRecord(string name, int iconId)
        {
            Name = name;
            IconId = iconId;
        }

        public override bool Equals(object obj)
        {
            return obj is IndividualRecord record && Name == record.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
    private static class IconCache<T>
    {
        public static T Cache { get; set; }

        public static bool HasValue => Cache is not null;
    }
    public static void RefreshContent<TImage>(
        IFormPerformanceAnalyzer<TImage> ui,
        Func<IGH_DocumentObject, TImage> iconGetter,
        PerformanceSnapshot snapshot,
        HashSet<Guid> preselected
        )
    {
        var doc = Instances.ActiveCanvas?.Document;
        if (doc == null)
            return;

        HashSet<Guid> focused = null;

        if (ui.FocusSelected)
        {
            if ((focused = preselected) is null)
                focused = new HashSet<Guid>(doc.SelectedObjects().Select(obj => obj.InstanceGuid));

            if (focused.Count == 0)
                focused = null;
        }

        ui.BeginVisualize();

        var totalTime = 0;
        var minorTime = 0;

        Dictionary<Guid, int> imageCache;

        switch (ui.Grouping)
        {
            case GroupCriteria.ByComponent:
                imageCache = new Dictionary<Guid, int>();

                foreach (var it in snapshot.ElapsedByComponent.OrderByDescending(comp => comp.Value))
                {
                    totalTime += it.Value;

                    if (it.Value <= MinimumElapse || !(focused?.Contains(it.Key) ?? true))
                    {
                        minorTime += it.Value;
                        continue;
                    }

                    var docObj = doc.FindObject(it.Key, true);
                    if (docObj is not IGH_ActiveObject) continue;

                    var compId = docObj.ComponentGuid;
                    var id = -1;
                    if (imageCache.TryGetValue(compId, out var index))
                    {
                        id = index;
                    }
                    else
                    {
                        id = ui.AddEntryImage(iconGetter(docObj));

                        imageCache[compId] = id;
                    }

                    var orgNickName = ObjectServer.GetOriginalNickName(compId);
                    var nickName = docObj.NickName;

                    var name = nickName != orgNickName && nickName != docObj.Name ? $"{nickName} ({docObj.Name})" : docObj.Name;

                    var entry = new PerformanceAnalyzerEntry
                    {
                        ObjectId = it.Key,
                        ObjectName = name,
                        ImageId = id,
                        DisplayTime = ToTimeString(it.Value),
                        DisplayPercentage = ToPercentage(it.Value, snapshot.TotalMilliseconds),
                    };

                    if (docObj is IGH_Component comp && comp.RunCount > 0)
                    {
                        entry.DisplayRunCount = comp.RunCount.ToString();
                        entry.DisplayTimePerRun = ToTimeString(it.Value / comp.RunCount);
                    }

                    ui.AddEntry(entry);
                }
                break;

            case GroupCriteria.ByGroup:

                if (!IconCache<TImage>.HasValue)
                    IconCache<TImage>.Cache = iconGetter(new GH_Group());

                ui.AddEntryImage(IconCache<TImage>.Cache);

                foreach (var it in snapshot.ElapsedByGroup.OrderByDescending(grp => grp.Value))
                {
                    totalTime += it.Value;

                    if (it.Value <= MinimumElapse || !(focused?.Contains(it.Key) ?? true))
                    {
                        minorTime += it.Value;
                        continue;
                    }

                    var group = doc.FindObject(it.Key, true);
                    if (group is not GH_Group) continue;
                    var name = string.IsNullOrEmpty(group.NickName) ? "Group" : group.NickName;

                    var entry = new PerformanceAnalyzerEntry
                    {
                        ObjectId = it.Key,
                        ObjectName = name,
                        ImageId = 0,
                        DisplayTime = ToTimeString(it.Value),
                        DisplayPercentage = ToPercentage(it.Value, snapshot.TotalMilliseconds),
                    };

                    ui.AddEntry(entry);
                }
                break;

            case GroupCriteria.ByCategory:
                imageCache = new Dictionary<Guid, int>();
                var records = new Dictionary<IndividualRecord, int>();

                foreach (var it in snapshot.ElapsedByComponent)
                {
                    totalTime += it.Value;

                    if (it.Value <= MinimumElapse || !(focused?.Contains(it.Key) ?? true))
                    {
                        minorTime += it.Value;
                        continue;
                    }

                    var docObj = doc.FindObject(it.Key, true);
                    if (docObj is not IGH_ActiveObject) continue;

                    var compId = docObj.ComponentGuid;
                    var id = -1;
                    if (imageCache.TryGetValue(compId, out var index))
                    {
                        id = index;
                    }
                    else
                    {
                        id = ui.AddEntryImage(iconGetter(docObj));

                        imageCache[compId] = id;
                    }

                    var orgNickName = ObjectServer.GetOriginalNickName(compId);
                    var nickName = docObj.NickName;

                    var name = nickName != orgNickName ? $"{nickName} ({docObj.Name})" : docObj.Name;

                    var record = new IndividualRecord(name, id);
                    if (records.TryGetValue(record, out var v))
                    {
                        records[record] = v + it.Value;
                    }
                    else
                    {
                        records[record] = it.Value;
                    }
                }

                foreach (var it in records.OrderByDescending(kv => kv.Value))
                {
                    var entry = new PerformanceAnalyzerEntry
                    {
                        ObjectId = Guid.Empty,
                        ObjectName = it.Key.Name,
                        ImageId = it.Key.IconId,
                        DisplayTime = ToTimeString(it.Value),
                        DisplayPercentage = ToPercentage(it.Value, snapshot.TotalMilliseconds),
                    };

                    ui.AddEntry(entry);
                }

                break;
        }

        // Add total time;

        ui.AddSpecialEntry(SpecialEntryType.EmptyRow, default);

        if (minorTime > 0)
        {
            ui.AddSpecialEntry(SpecialEntryType.MinorComponents, new PerformanceAnalyzerEntry
            {
                ObjectId = Guid.Empty,
                ObjectName = Strings.OtherComponents,
                ImageId = PerformanceAnalyzerEntry.NO_IMAGE,
                DisplayTime = ToTimeString(minorTime),
                DisplayPercentage = ToPercentage(minorTime, snapshot.TotalMilliseconds),
            });
        }

        ui.AddSpecialEntry(SpecialEntryType.Total, new PerformanceAnalyzerEntry
        {
            ObjectId = Guid.Empty,
            ObjectName = "Total",
            ImageId = PerformanceAnalyzerEntry.NO_IMAGE,
            DisplayTime = ToTimeString(totalTime)
        });

        ui.EndVisualize();
    }

    private static string ToPercentage(int part, int whole)
    {
        if (part == 0) return whole == 0 ? "100.0" : "0.0";
        return $"{100.0 * part / whole:0.0}";
    }
    private static string ToTimeString(int ms)
    {
        if (MiscConfig.ShowSecondsInPerformanceAnalyzer)
        {
            return $"{ms / 1000.0:0.0} s";
        }
        else
        {
            if (ms >= 1000)
            {
                const int SeparateChar = 3;

                var timeStr = ms.ToString();
                var str = "ms";

                for (var i = timeStr.Length - SeparateChar; i > -SeparateChar; i -= 3)
                {
                    if (i < 0)
                    {
                        str = timeStr.Substring(0, i + SeparateChar) + " " + str;
                    }
                    else
                    {
                        str = timeStr.Substring(i, SeparateChar) + " " + str;
                    }
                }

                return str;
            }
            else
            {
                return $"{ms} ms";
            }
        }
    }
}
