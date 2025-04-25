using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public sealed class GH_Structure<T> : IGH_Structure
    where T : IGH_Goo
{
    private readonly struct Enumerator(IEnumerable<IGH_Goo> goos) : IGH_StructureEnumerator
    {
        private readonly IEnumerable<IGH_Goo> _goos = goos;
        public IEnumerator<IGH_Goo> GetEnumerator() => _goos.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private readonly SortedList<GH_Path, List<T>> _v = [];
    public bool IsEmpty => _v.Count == 0;

    public int PathCount => _v.Keys.Count;

    public int DataCount => _v.Sum(static kv => kv.Value.Count);

    public IList<GH_Path> Paths => _v.Keys;

    public IList<IList> StructureProxy => _v.Select(kv => (IList)kv.Value).ToList();

    public string TopologyDescription => "";
    public IList<List<T>> Branches => _v.Values;
    public IEnumerable<T> NonNulls => [.. _v.SelectMany(static kv => kv.Value.Where(static x => x is not null))];
    public List<T> this[int index] => _v.Values[index];
    public List<T>? this[GH_Path path] => _v.TryGetValue(path, out var v) ? v : null;

    public T FirstItem => NonNulls.FirstOrDefault();
    public IGH_StructureEnumerator AllData(bool skipNulls)
    {
        if (skipNulls)
        {
            return new Enumerator(_v.SelectMany(static kv => kv.Value.Cast<IGH_Goo>().Where(static x => x is not null)));
        }
        else
        {
            return new Enumerator(_v.SelectMany(static kv => kv.Value.Cast<IGH_Goo>()));
        }
    }

    public void Clear() => _v.Clear();

    public void ClearData()
    {
        foreach (var kv in _v) kv.Value.Clear();
    }

    public string DataDescription(bool includeIndices, bool includePaths)
    {
        return "";
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity < 8) capacity = 8;
        _v.Capacity = Math.Max(_v.Capacity, capacity);
    }

    public void Flatten(GH_Path? path = null)
    {
        path ??= new(0);

        var list = new List<T>();
        foreach (var kv in _v)
        {
            list.AddRange(kv.Value);
        }

        Clear();
        EnsurePath(path).AddRange(list);
    }

    public IList get_Branch(int index) => _v.Values[index];

    public IList get_Branch(GH_Path path) => _v[path];

    public GH_Path get_Path(int index) => _v.Keys[index];

    public bool PathExists(GH_Path path) => _v.ContainsKey(path);

    public void RemovePath(GH_Path path) => _v.Remove(path);

    public void ReplacePath(GH_Path find, GH_Path replace)
    {
        var ind = _v.IndexOfKey(find);
        if (ind < 0) return;
        var list = _v.Values[ind];
        _v.RemoveAt(ind);
        _v[replace] = list;
    }

    public List<T> EnsurePath(params int[] path)
    {
        return EnsurePath(new GH_Path(path));
    }
    public List<T> EnsurePath(GH_Path path)
    {
        if (path.Length == 0)
        {
            path = new GH_Path(0);
        }

        if (_v.TryGetValue(path, out var v))
        {
            return v;
        }

        return _v[path] = [];
    }


}