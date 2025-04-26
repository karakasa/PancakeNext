using Grasshopper2.Data;
using Grasshopper2.Parameters.Special;
using OneCodeTwoVersions.Polyfill.G2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public sealed class DataTree<T> : IGH_DataTree
{
    private SortedList<GH_Path, List<T>> _v = [];
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

    private List<T> LastOrDefaultBranch => BranchCount == 0 ? _v[new(0)] = [] : _v.Values[_v.Count - 1];
    public void Add(T value)
    {
        LastOrDefaultBranch.Add(value);
    }
    public void Add(T value, GH_Path path) => EnsurePath(path).Add(value);

    public void AddRange(IEnumerable<T> values) => LastOrDefaultBranch.AddRange(values);
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
    public List<T> EnsurePath(GH_Path? path)
    {
        if (path is null || path.Length == 0)
        {
            path = new GH_Path(0);
        }

        if (_v.TryGetValue(path, out var v))
        {
            return v;
        }

        return _v[path] = [];
    }

    public void TrimExcess()
    {
        foreach (var kv in _v)
            kv.Value.TrimExcess();
    }
    public void RenumberPaths()
    {
        var listNew = new SortedList<GH_Path, List<T>>();
        var ind = 0;
        foreach (var v in _v)
        {
            listNew.Add(new GH_Path(ind), v.Value);
            ++ind;
        }
        _v = listNew;
    }
    public void Graft(GH_Path path, bool skipNulls = false)
    {
        if (!_v.TryGetValue(path, out var list)) return;

        var ind = -1;
        foreach (var v in list)
        {
            ++ind;
            if (skipNulls && v is null) continue;

            var newPath = path.AppendElement(ind);
            EnsurePath(newPath).Add(v);
        }

        _v.Remove(path);
    }

    public void Graft(bool skipNulls = false)
    {
        foreach (var path in _v.Keys.ToArray())
            Graft(path, skipNulls);
    }

    public void MergeTree(DataTree<T> other)
    {
        if (other == null) return;

        foreach (var kv in other._v)
            AddRange(kv.Value, kv.Key);
    }
    public void MergeStructure(IGH_Structure? other, IGH_TypeHint? hint)
    {
        if (other is null) return;

        var cnt = other.PathCount;
        for (var i = 0; i < cnt; i++)
        {
            var list = EnsurePath(other.get_Path(i));
            var branch = other.get_Branch(i);

            foreach (var item in branch)
                list.Add(Convert(item, hint));
        }
    }

    private static T Convert(object data, IGH_TypeHint? hint)
    {
        if (data is null) return default!;
        if (data is T t) return t;
        if (hint is null)
        {
            if (data is IGH_Goo goo && goo.CastTo(out t))
            {
                return t;
            }
        }
        else
        {
            if (hint.Cast(data, out var obj) && obj is T t2)
            {
                return t2;
            }
        }

        return default!;
    }

    public bool MergeWithParameter(IGH_Param param)
    {
        throw new NotSupportedException("This method is not supported in compatibility middleware. The middleware would use other ways to create trees.");
    }

    public DataTree() { }
    public DataTree(IEnumerable<T>? data, GH_Path? root = null) { EnsurePath(root).AddRange(data ?? []); }
    public DataTree(DataTree<T> other)
    {
        foreach (var kv in other._v)
            AddRange(kv.Value, kv.Key);
    }
    public DataTree(DataTree<T> other, Func<T, T> cloner)
    {
        foreach (var kv in other._v)
        {
            var list = EnsurePath(kv.Key);
            foreach (var it in kv.Value)
                list.Add(cloner(it));
        }
    }
    public Tree<T> To2() => Garden.TreeFromTwigs(new(_v.Keys.Select(static p => p.To2())), _v.Values.Select(v => Garden.TwigFromList(v)));
    ITree IGH_DataTree.To2() => To2();
}
