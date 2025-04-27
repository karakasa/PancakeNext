#if G2
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Types.Constraints.Goals;
using OneCodeTwoVersions.Polyfill.DataTypes;
using Rhino.Geometry;
using System.Collections;
using System.Drawing;
using System.Reflection;

namespace OneCodeTwoVersions.Polyfill;
internal sealed class DataAccessWrapper(GH_Component comp) : IGH_DataAccess
{
    private readonly GH_Component _comp = comp;
    private IDataAccess Access => _comp.CurrentAccess!;
    public int Iteration => Access.Iterations;

    private int FindInput(string name) => FindParamByName(_comp.Parameters.Inputs, name);
    private int FindOutput(string name) => FindParamByName(_comp.Parameters.Outputs, name);
    private static int FindParamByName(IEnumerable<IParameter> ps, string name)
    {
        var ind = 0;

        foreach (var p in ps)
        {
            if (p.Nomen.Name == name)
            {
                return ind;
            }

            ++ind;
        }

        return -1;
    }
    public void AbortComponentSolution()
    {
        _comp.Document.Solution.Stop();
    }

    public bool GetData<T1>(int index, ref T1 destination)
    {
        return Access.GetItem(index, out destination);
    }

    public bool GetData<T1>(string name, ref T1 destination)
    {
        return GetData(FindInput(name), ref destination);
    }

    public bool GetDataList<T1>(int index, List<T1> list)
    {
        if (!Access.GetTwig<T1>(index, out var twig)) return false;

        list.Clear();
        foreach (var it in twig.Pears)
        {
            list.Add(it is null ? default! : it.Item);
        }
        return true;
    }

    public bool GetDataList<T1>(string name, List<T1> list)
    {
        return GetDataList(FindInput(name), list);
    }

    public bool SetData(int paramIndex, object data)
    {
        Access.SetItem(paramIndex, data.Peel());
        return true;
    }

    public bool SetData(int paramIndex, object data, int itemIndexOverride)
    {
        throw new NotSupportedException();
    }

    public bool SetData(string paramName, object data)
    {
        return SetData(FindOutput(paramName), data);
    }

    public bool SetDataList(int paramIndex, IEnumerable data)
    {
        Access.SetTwig(paramIndex, Garden.ITwigFromList(data));
        return true;
    }

    public bool SetDataList(int paramIndex, IEnumerable data, int listIndexOverride)
    {
        throw new NotSupportedException();
    }

    public bool SetDataList(string paramName, IEnumerable data)
    {
        return SetDataList(FindOutput(paramName), data);
    }

    public bool SetDataTree(int paramIndex, IGH_DataTree tree)
    {
        Access.SetTree(paramIndex, tree.To2());
        return true;
    }

    public bool SetDataTree(int paramIndex, IGH_Structure tree)
    {
        Access.SetTree(paramIndex, tree.To2());
        return true;
    }

    public bool GetDataTree<T1>(string name, out GH_Structure<T1>? tree) where T1 : IGH_Goo
    {
        return GetDataTree(FindInput(name), out tree);
    }

    private static readonly Dictionary<Type, object> EmptyGooCache = [];

    private static TGoo GetEmptyInstance<TGoo>() where TGoo : IGH_Goo
    {
        if (!EmptyGooCache.TryGetValue(typeof(TGoo), out var k))
            EmptyGooCache[typeof(TGoo)] = k = CreateEmptyInstance<TGoo>();
        return (TGoo)k;
    }
    private static TGoo CreateEmptyInstance<TGoo>() where TGoo : IGH_Goo
    {
        var type = typeof(TGoo);
        if (type.IsAbstract) return default;
        var ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, [], null);
        if (ctor is null) return default;

        return (TGoo)ctor.Invoke(null);
    }

    public bool GetDataTree<T1>(int index, out GH_Structure<T1>? tree) where T1 : IGH_Goo
    {
        tree =
            TryConvertDataTree<int, GH_Integer, T1>(index) ??
            TryConvertDataTree<bool, GH_Boolean, T1>(index) ??
            TryConvertDataTree<double, GH_Number, T1>(index) ??
            TryConvertDataTree<Vector3d, GH_Vector, T1>(index) ??
            TryConvertDataTree<Transform, GH_Transform, T1>(index) ??
            TryConvertDataTree<Plane, GH_Plane, T1>(index) ??
            TryConvertDataTree<Box, GH_Box, T1>(index) ??
            TryConvertDataTree<Line, GH_Line, T1>(index) ??
            TryConvertDataTree<Circle, GH_Circle, T1>(index) ??
            TryConvertDataTree<Rectangle3d, GH_Rectangle, T1>(index) ??
            TryConvertDataTree<Arc, GH_Arc, T1>(index) ??
            TryConvertDataTree<Curve, GH_Curve, T1>(index) ??
            TryConvertDataTree<Surface, GH_Surface, T1>(index) ??
            TryConvertDataTree<Brep, GH_Brep, T1>(index) ??
            TryConvertDataTree<SubD, GH_SubD, T1>(index) ??
            TryConvertDataTree<Mesh, GH_Mesh, T1>(index) ??
            TryConvertDataTree<MeshFace, GH_MeshFace, T1>(index) ??
            TryConvertDataTree<DateTime, GH_Time, T1>(index) ??
            TryConvertDataTree<Color, GH_Colour, T1>(index) ??
            TryConvertDataTree<string, GH_Text, T1>(index) ??
            TryConvertDataTree<Interval, GH_Interval, T1>(index) ??
            TryConvertDataTree<GeometryBase, GH_Geometry, T1>(index) ??
            TryConvertDataTree<GH_Path, GH_StructurePath, T1, Grasshopper2.Data.Path>(index, x => x.To1()) ??
            TryConvertDataTreeGeneric<T1>(index) ??
            TryConvertDataTreeWithCast<T1>(index);

        return tree is not null;
    }

    private GH_Structure<TInput>? TryConvertDataTreeGeneric<TInput>(int index)
        where TInput : IGH_Goo
    {
        if (typeof(TInput) != typeof(IGH_Goo)) return null;
        if (!Access.GetITree(index, out var tree)) return null;

        return (GH_Structure<TInput>)(object)tree.To1();
    }

    private GH_Structure<TInput>? TryConvertDataTreeWithCast<TInput>(int index)
        where TInput : IGH_Goo
    {
        if (GetEmptyInstance<TInput>() is not { } emptyGoo) return null;
        if (!Access.GetITree(index, out var tree)) return null;

        var tree1 = new GH_Structure<TInput>();
        var duplicated = default(TInput);

        for (var i = 0; i < tree.PathCount; i++)
        {
            var path = tree.Paths[i];
            var twig = tree.Twigs[i];
            var list = tree1.EnsurePath(path.To1());
            var cnt = twig.LeafCount;
            for (var j = 0; j < cnt; j++)
            {
                if (twig.NullAt(j))
                {
                    list.Add(default);
                }
                else
                {
                    duplicated ??= (TInput)emptyGoo.Duplicate();
                    if (duplicated is null || object.ReferenceEquals(duplicated, emptyGoo))
                        throw new InvalidOperationException($"{typeof(TInput).Name}.Duplicate() is not well defined.");

                    if (duplicated.CastFrom(twig[j].Item))
                    {
                        list.Add(duplicated);
                        duplicated = default;
                    }
                    else
                    {
                        list.Add(default);
                    }
                }
            }
        }

        return tree1;
    }

    private GH_Structure<TInput>? TryConvertDataTree<TData, TGoo, TInput>(int index)
        where TGoo : GH_Goo<TData>
        where TInput : IGH_Goo
    {
        if (typeof(TInput) != typeof(TGoo)) return null;
        if (!Access.GetTree<TData>(index, out var tree)) return null;

        var tree1 = new GH_Structure<TInput>();

        for (var i = 0; i < tree.PathCount; i++)
        {
            var path = tree.Paths[i];
            var twig = tree.Twigs[i];
            var list = tree1.EnsurePath(path.To1());
            var cnt = twig.LeafCount;
            for (var j = 0; j < cnt; j++)
            {
                if (twig.NullAt(j))
                {
                    list.Add(default);
                }
                else
                {
                    list.Add((TInput)PolyfillExtensions.Create(twig[j].Item));
                }
            }
        }

        return tree1;
    }

    private GH_Structure<TInput>? TryConvertDataTree<TData, TGoo, TInput, TData2>(int index, Func<TData2, TData> converter)
        where TGoo : GH_Goo<TData>
        where TInput : IGH_Goo
    {
        if (typeof(TInput) != typeof(TGoo)) return null;
        if (!Access.GetTree<TData2>(index, out var tree)) return null;

        var tree1 = new GH_Structure<TInput>();

        for (var i = 0; i < tree.PathCount; i++)
        {
            var path = tree.Paths[i];
            var twig = tree.Twigs[i];
            var list = tree1.EnsurePath(path.To1());
            var cnt = twig.LeafCount;
            for (var j = 0; j < cnt; j++)
            {
                if (twig.NullAt(j))
                {
                    list.Add(default);
                }
                else
                {
                    list.Add((TInput)PolyfillExtensions.Create(converter(twig[j].Item)));
                }
            }
        }

        return tree1;
    }
}

#endif