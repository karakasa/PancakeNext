#if G2
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using OneCodeTwoVersions.Polyfill.DataTypes;
using Rhino.Geometry;
using System.Collections;
using System.Drawing;

namespace OneCodeTwoVersions.Polyfill;
public abstract partial class ComponentMiddleware<TComp> where TComp : ComponentMiddleware<TComp>
{
    private sealed class DataAccessWrapper(ComponentMiddleware<TComp> comp) : IGH_DataAccess
    {
        private readonly ComponentMiddleware<TComp> _comp = comp;
        private IDataAccess Access => _comp._currentAccess!;
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

        public bool GetDataTree<T1>(string name, out GH_Structure<T1> tree) where T1 : IGH_Goo
        {
            return GetDataTree(FindInput(name), out tree);
        }

        public bool GetDataTree<T1>(int index, out GH_Structure<T1> tree) where T1 : IGH_Goo
        {
            tree = null;

            if (
                TryConvertDataTree<int, GH_Integer, T1>(index, ref tree) ||
                TryConvertDataTree<bool, GH_Boolean, T1>(index, ref tree) ||
                TryConvertDataTree<double, GH_Number, T1>(index, ref tree) ||
                TryConvertDataTree<Vector3d, GH_Vector, T1>(index, ref tree) ||
                TryConvertDataTree<Transform, GH_Transform, T1>(index, ref tree) ||
                TryConvertDataTree<Plane, GH_Plane, T1>(index, ref tree) ||
                TryConvertDataTree<Box, GH_Box, T1>(index, ref tree) ||
                TryConvertDataTree<Line, GH_Line, T1>(index, ref tree) ||
                TryConvertDataTree<Circle, GH_Circle, T1>(index, ref tree) ||
                TryConvertDataTree<Rectangle3d, GH_Rectangle, T1>(index, ref tree) ||
                TryConvertDataTree<Arc, GH_Arc, T1>(index, ref tree) ||
                TryConvertDataTree<Curve, GH_Curve, T1>(index, ref tree) ||
                TryConvertDataTree<Surface, GH_Surface, T1>(index, ref tree) ||
                TryConvertDataTree<Brep, GH_Brep, T1>(index, ref tree) ||
                TryConvertDataTree<SubD, GH_SubD, T1>(index, ref tree) ||
                TryConvertDataTree<Mesh, GH_Mesh, T1>(index, ref tree) ||
                TryConvertDataTree<MeshFace, GH_MeshFace, T1>(index, ref tree) ||
                TryConvertDataTree<DateTime, GH_Time, T1>(index, ref tree) ||
                TryConvertDataTree<Color, GH_Colour, T1>(index, ref tree) ||
                TryConvertDataTree<string, GH_Text, T1>(index, ref tree) ||
                TryConvertDataTree<Interval, GH_Interval, T1>(index, ref tree) ||
                TryConvertDataTree<GeometryBase, GH_Geometry, T1>(index, ref tree) ||
                TryConvertDataTree<GH_Path, GH_StructurePath, T1, Grasshopper2.Data.Path>(index, ref tree, x => x.To1()) ||
                TryConvertDataTreeGeneric<T1>(index, ref tree)
                )
            {
                return true;
            }

            tree = null;
            return false;
        }

        private bool TryConvertDataTreeGeneric<TInput>(int index, ref GH_Structure<TInput?> gh1Tree)
            where TInput : IGH_Goo
        {
            if (typeof(TInput) != typeof(IGH_Goo)) return false;
            if (!Access.GetITree(index, out var tree)) return false;

            gh1Tree = (GH_Structure<TInput>)(object)tree.To1();
            return true;
        }

        private bool TryConvertDataTree<TData, TGoo, TInput>(int index, ref GH_Structure<TInput?> gh1Tree)
            where TGoo : GH_Goo<TData>
            where TInput : IGH_Goo
        {
            if (typeof(TInput) != typeof(TGoo)) return false;
            if (!Access.GetTree<TData>(index, out var tree)) return false;

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

            gh1Tree = tree1;
            return true;
        }

        private bool TryConvertDataTree<TData, TGoo, TInput, TData2>(int index, ref GH_Structure<TInput?> gh1Tree, Func<TData2, TData> converter)
            where TGoo : GH_Goo<TData>
            where TInput : IGH_Goo
        {
            if (typeof(TInput) != typeof(TGoo)) return false;
            if (!Access.GetTree<TData2>(index, out var tree)) return false;

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

            gh1Tree = tree1;
            return true;
        }

       
    }
}

#endif