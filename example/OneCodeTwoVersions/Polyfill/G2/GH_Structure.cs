using Grasshopper2.Data;
using Grasshopper2.Types.Conversion;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Path = Grasshopper2.Data.Path;

namespace OneCodeTwoVersions.Polyfill;
public sealed class GH_Structure<T> : IGH_Structure, IEnumerable<T>
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

    private IEnumerable<T> AllData() => _v.SelectMany(static kv => kv.Value);

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
    private List<T> LastOrDefaultBranch => PathCount == 0 ? _v[new(0)] = [] : _v.Values[_v.Count - 1];
    public void Append(T data) => LastOrDefaultBranch.Add(data);
    public void Append(T data, GH_Path? path) => EnsurePath(path).Add(data);
    public void AppendRange(IEnumerable<T> data) => LastOrDefaultBranch.AddRange(data);
    public void AppendRange(IEnumerable<T> data, GH_Path path) => EnsurePath(path).AddRange(data);
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

    public void MergeStructure(GH_Structure<T> other)
    {
        foreach (var kv in other._v)
            AppendRange(kv.Value, kv.Key);
    }
    public void Reverse()
    {
        foreach (var kv in _v)
            kv.Value.Reverse();
    }
    public GH_Structure<T> Duplicate() => new(this, false);
    public GH_Structure<TTo> DuplicateCast<TTo>(Func<T, TTo> conversion) where TTo : IGH_Goo
    {
        var dest = new GH_Structure<TTo>();

        foreach (var kv in _v)
        {
            var list = dest.EnsurePath(kv.Key);
            foreach (var item in kv.Value)
            {
                list.Add(item is null ? default! : conversion(item));
            }
        }

        return dest;
    }

    public IEnumerator<T> GetEnumerator() => AllData().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public GH_Structure() { }
    public GH_Structure(GH_Structure<T>? other, bool shallowCopy)
    {
        if (other?.IsEmpty ?? true) { return; }

        foreach (var kv in other._v)
        {
            var list = EnsurePath(kv.Key);
            foreach (var item in kv.Value)
            {
                list.Add((shallowCopy || item is null) ? item! : (T)item.Duplicate());
            }
        }
    }

    public ITree? To2<TGoo, TGh1Inner, TGh2Inner>(Func<TGh1Inner, TGh2Inner> converter, bool allowGeneric = true)
        where TGoo : GH_Goo<TGh1Inner>
    {
        ITree? ret = null;
        var paths = new Paths(_v.Keys.Select(static p => p.To2()));

        return CreateTreeFastPath<TGh1Inner, TGoo, TGh2Inner>(ref ret, paths, converter) ? ret : null;
    }
    public ITree? To2() => To2(true);
    public ITree? To2(bool allowGeneric)
    {
        ITree? ret = null;
        var paths = new Paths(_v.Keys.Select(static p => p.To2()));

        if (
            CreateTreeFastPath<int, GH_Integer>(ref ret, paths) ||
            CreateTreeFastPath<bool, GH_Boolean>(ref ret, paths) ||
            CreateTreeFastPath<double, GH_Number>(ref ret, paths) ||
            CreateTreeFastPath<string, GH_Text>(ref ret, paths) ||
            CreateTreeFastPath<Point3d, GH_Point>(ref ret, paths) ||
            CreateTreeFastPath<Vector3d, GH_Vector>(ref ret, paths) ||
            CreateTreeFastPath<Transform, GH_Transform>(ref ret, paths) ||
            CreateTreeFastPath<Plane, GH_Plane>(ref ret, paths) ||
            CreateTreeFastPath<Box, GH_Box>(ref ret, paths) ||
            CreateTreeFastPath<Line, GH_Line>(ref ret, paths) ||
            CreateTreeFastPath<Circle, GH_Circle>(ref ret, paths) ||
            CreateTreeFastPath<Rectangle3d, GH_Rectangle>(ref ret, paths) ||
            CreateTreeFastPath<Arc, GH_Arc>(ref ret, paths) ||
            CreateTreeFastPath<Curve, GH_Curve>(ref ret, paths) ||
            CreateTreeFastPath<Surface, GH_Surface>(ref ret, paths) ||
            CreateTreeFastPath<Brep, GH_Brep>(ref ret, paths) ||
            CreateTreeFastPath<SubD, GH_SubD>(ref ret, paths) ||
            CreateTreeFastPath<Mesh, GH_Mesh>(ref ret, paths) ||
            CreateTreeFastPath<MeshFace, GH_MeshFace>(ref ret, paths) ||
            CreateTreeFastPath<DateTime, GH_Time>(ref ret, paths) ||
            CreateTreeFastPath<Color, GH_Colour>(ref ret, paths) ||
            CreateTreeFastPath<Interval, GH_Interval>(ref ret, paths) ||
            CreateTreeFastPath<GeometryBase, GH_Geometry>(ref ret, paths) ||
            CreateTreeFastPath<object, GH_ObjectWrapper>(ref ret, paths) ||
            CreateTreeFastPath<GH_Path, GH_StructurePath, Path>(ref ret, paths, ConvertPath)
            )
        {
            return ret;
        }

        if (allowGeneric)
        {
            return Garden.ITreeFromITwigs(paths, _v.Values.Select(v => Garden.ITwigFromList(v.Select(a => a?.ScriptVariable()))));
        }
        else
        {
            return null;
        }
    }

    private Path ConvertPath(GH_Path data) => data.To2();

    private bool CreateTreeFastPath<TData, TGoo>(ref ITree? tree, Paths paths) where TGoo : GH_Goo<TData>
    {
        if (typeof(T) != typeof(TGoo)) return false;

        var list = new List<Twig<TData>>();

        foreach (var kv in _v)
        {
            var factory = new TwigFactory<TData>(kv.Value.Count);
            foreach (var it in kv.Value)
            {
                var goo = (TGoo)(object)it;
                if (goo is null)
                {
                    factory.AddNull();
                }
                else
                {
                    factory.Add(goo.Value);
                }
            }

            list.Add(factory.Create());
        }

        tree = Garden.TreeFromTwigs(paths, list);
        return true;
    }
    private bool CreateTreeFastPath<TData, TGoo, TOutput>(ref ITree? tree, Paths paths, Func<TData, TOutput> converter) where TGoo : GH_Goo<TData>
    {
        if (typeof(T) != typeof(TGoo)) return false;

        var list = new List<Twig<TOutput>>();

        foreach (var kv in _v)
        {
            var factory = new TwigFactory<TOutput>(kv.Value.Count);
            foreach (var it in kv.Value)
            {
                var goo = (TGoo)(object)it;
                if (goo is null)
                {
                    factory.AddNull();
                }
                else
                {
                    factory.Add(converter(goo.Value));
                }
            }

            list.Add(factory.Create());
        }

        tree = Garden.TreeFromTwigs(paths, list);
        return true;
    }
}