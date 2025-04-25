using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public sealed class DataTree<T> : IGH_DataTree
{
    private readonly SortedList<GH_Path, List<T>> _v = [];
    public int BranchCount => _v.Keys.Count;

    public int DataCount => _v.Sum(static kv => kv.Value.Count);

    public IList<GH_Path> Paths => _v.Keys;

    public string TopologyDescription => "";
    public IList<List<T>> Branches => _v.Values;
    public T this[GH_Path path, int index]
    {
        get => _v[path][index];
        set => _v[path][index] = value;
    }

    private List<T> FirstOrDefaultBranch => BranchCount == 0 ? _v[new(0)] = new List<T>() : _v.Values[0];
    public void Add(T value)
    {
        FirstOrDefaultBranch.Add(value);
    }
    public void Add(T value, GH_Path path) => EnsurePath(path).Add(value);

    public void AddRange(IEnumerable<T> values) => FirstOrDefaultBranch.AddRange(values);
    public void AddRange(IEnumerable<T> values, GH_Path path) => EnsurePath(path).AddRange(values);

    public List<T> Branch(int index) => _v.Values[index];
    public List<T> Branch(GH_Path path) => _v[path];
    public List<T> Branch(params int[] path) => _v[new(path)];
    public bool ItemExists(GH_Path path, int index)
    {
        if (index < 0 || !_v.TryGetValue(path, out var list) || list == null) { return false; }
        return index < list.Count;
    }
    public void Insert(T data, GH_Path path, int index)
    {
        var list = EnsurePath(path);
        if (index <= list.Count)
        {
            list.Insert(index, data);
        }
        else
        {
            list.AddRange(Enumerable.Repeat(default(T)!, index - list.Count));
            list.Add(data);
        }
    }
    public GH_Path Path(int index) => _v.Keys[index];

    public List<T> AllData()
    {
        return [.. _v.SelectMany(static kv => kv.Value)];
    }

    public void Clear() => _v.Clear();

    public void ClearData()
    {
        foreach (var kv in _v) kv.Value.Clear();
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

    public bool PathExists(GH_Path path) => _v.ContainsKey(path);
    public bool PathExists(params int[] path) => PathExists(new GH_Path(path));

    public void RemovePath(GH_Path path) => _v.Remove(path);
    public void RemovePath(params int[] path) => RemovePath(new GH_Path(path));

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

    public bool MergeWithParameter(IGH_Param param)
    {
        throw new NotImplementedException();
    }
}
