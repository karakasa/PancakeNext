#if G2
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using System.Collections;

namespace OneCodeTwoVersions.Polyfill;
public abstract partial class ComponentMiddleware<T> where T : ComponentMiddleware<T>
{
    private sealed class DataAccessWrapper(ComponentMiddleware<T> comp) : IGH_DataAccess
    {
        private readonly ComponentMiddleware<T> _comp = comp;
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
            Access.SetItem(paramIndex, data);
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
            throw new NotImplementedException();
        }

        public bool SetDataTree(int paramIndex, IGH_Structure tree)
        {
            throw new NotImplementedException();
        }

        public bool GetDataTree<T1>(int index, out GH_Structure<T1> tree) where T1 : IGH_Goo
        {
            throw new NotImplementedException();
        }

        public bool GetDataTree<T1>(string name, out GH_Structure<T1> tree) where T1 : IGH_Goo
        {
            throw new NotImplementedException();
        }
    }
}

#endif