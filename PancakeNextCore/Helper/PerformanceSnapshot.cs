using Grasshopper2.Doc;
using System;
using System.Collections.Generic;
using System.Linq;
using PancakeNextCore.Utility;
using Grasshopper2.UI;
using Grasshopper2.SpecialObjects;

namespace PancakeNextCore.Helper;

public sealed class PerformanceSnapshot
{
    private readonly Dictionary<Guid, int> _elapsed = [];
    private readonly Dictionary<Guid, Guid[]> _groups = [];
    public int TotalMilliseconds { get; private set; } = 0;
    public string? Name { get; set; }
    public DateTime SnapshotTime { get; private set; }

    public IEnumerable<KeyValuePair<Guid, int>> ElapsedByGroup
    {
        get
        {
            foreach (var it in _groups)
            {
                var sum = 0;
                foreach (var it2 in it.Value)
                {
                    if (_elapsed.TryGetValue(it2, out var elapsed))
                        sum += elapsed;
                }
                yield return new KeyValuePair<Guid, int>(it.Key, sum);
            }
        }
    }

    public IEnumerable<KeyValuePair<Guid, int>> ElapsedByComponent => _elapsed;

    private static IEnumerable<T> RecurGroup<T>(GroupObject grp)
    {
        foreach (var it in grp.Content)
        {
            if (it is GroupObject child)
            {
                foreach (var it2 in RecurGroup<T>(child))
                    yield return it2;
            }
            else if (it is T t)
            {
                yield return t;
            }
        }
    }

    private void ReadGroups(Document doc)
    {
        foreach (var grp in doc.Objects.Groups)
        {
            var objGuids = RecurGroup<ActiveObject>(grp).Select(obj => obj.InstanceId).ToArray();
            _groups[grp.InstanceId] = objGuids;
        }
    }

    private void ReadTimeSpan(Document doc)
    {
        var sum = 0;

        foreach (var it in doc.Objects.ActiveObjects.OfType<ActiveObject>())
        {
            var time = (int)(it.GetDuration() ?? 0);
            if (_elapsed.TryGetValue(it.InstanceId, out var currentTime))
            {
                _elapsed[it.InstanceId] = time + currentTime;
            }
            else
            {
                _elapsed[it.InstanceId] = time;
            }
            sum += time;
        }

        TotalMilliseconds += sum;
    }

    public PerformanceSnapshot(Document doc)
    {
        SnapshotTime = DateTime.Now;
        ReadTimeSpan(doc);
        ReadGroups(doc);
    }

    private PerformanceSnapshot()
    {
        SnapshotTime = DateTime.Now;
    }

    public static PerformanceSnapshot? CreateFromDoc(Document? doc = null)
    {
        doc ??= Editor.Instance?.Canvas?.Document;

        if (doc == null)
            return null;

        return new PerformanceSnapshot(doc);
    }

    public static async Task<PerformanceSnapshot?> CreateFromDocAverage(Func<bool> shouldContinue, Document? doc = null)
    {
        doc ??= Editor.Instance?.Canvas?.Document;

        if (doc == null)
            return null;

        var snapshot = new PerformanceSnapshot();
        snapshot.ReadGroups(doc);

        var cnt = 0;

        while (shouldContinue())
        {
            await doc.Solution.Start();

            if (doc.Solution.State == SolutionState.Cancelled)
                return null;

            snapshot.ReadTimeSpan(doc);
            ++cnt;
        }

        if (cnt == 0)
            return null;

        if (cnt != 1)
        {
            snapshot.TotalMilliseconds /= cnt;
            foreach (var it in snapshot._elapsed.Keys.ToArray())
            {
                snapshot._elapsed[it] /= cnt;
            }
        }

        return snapshot;
    }
}
