using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Pancake.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PancakeNextCore.Helper;

public class PerformanceSnapshot
{
    private readonly Dictionary<Guid, string> _states = new Dictionary<Guid, string>();
    private readonly Dictionary<Guid, int> _elapsed = new Dictionary<Guid, int>();
    private readonly Dictionary<Guid, Guid[]> _groups = new Dictionary<Guid, Guid[]>();
    public int TotalMilliseconds { get; private set; } = 0;
    public string Name { get; set; }
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
    private void ReadStates(GH_Document doc, IEnumerable<IGH_DocumentObject> objs = null)
    {
        if (objs == null) return;

        objs.DoForEach(obj =>
        {
            if (obj is IGH_StateAwareObject stateAware)
                _states[obj.InstanceGuid] = stateAware.SaveState();
        });
    }

    private void ReadGroups(GH_Document doc)
    {
        foreach (var grp in doc.Objects.OfType<GH_Group>())
        {
            var objGuids = grp.ObjectsRecursive().Select(obj => obj.InstanceGuid).ToArray();
            _groups[grp.InstanceGuid] = objGuids;
        }
    }

    private void ReadTimeSpan(GH_Document doc)
    {
        var sum = 0;

        foreach (var it in doc.Objects.OfType<IGH_ActiveObject>())
        {
            var time = (int)it.ProcessorTime.TotalMilliseconds;
            if (_elapsed.TryGetValue(it.InstanceGuid, out var currentTime))
            {
                _elapsed[it.InstanceGuid] = time + currentTime;
            }
            else
            {
                _elapsed[it.InstanceGuid] = time;
            }
            sum += time;
        }

        TotalMilliseconds += sum;
    }

    public PerformanceSnapshot(GH_Document doc, IEnumerable<IGH_DocumentObject> stateObjs)
    {
        SnapshotTime = DateTime.Now;
        ReadStates(doc, stateObjs);
        ReadTimeSpan(doc);
        ReadGroups(doc);
    }

    public PerformanceSnapshot(GH_Document doc) : this(doc, null)
    {
    }

    private PerformanceSnapshot()
    {
        SnapshotTime = DateTime.Now;
    }

    public static PerformanceSnapshot CreateFromDoc(GH_Document doc = null)
    {
        if (doc == null)
            doc = Instances.ActiveCanvas?.Document;

        if (doc == null)
            return null;

        return new PerformanceSnapshot(doc);
    }

    public static PerformanceSnapshot CreateFromDocAverage(Func<bool> shouldContinue, GH_Document doc = null)
    {
        if (doc == null)
            doc = Instances.ActiveCanvas?.Document;

        if (doc == null)
            return null;

        var snapshot = new PerformanceSnapshot();
        snapshot.ReadGroups(doc);

        var cnt = 0;

        while (shouldContinue())
        {
            doc.NewSolution(true, GH_SolutionMode.Silent);
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
