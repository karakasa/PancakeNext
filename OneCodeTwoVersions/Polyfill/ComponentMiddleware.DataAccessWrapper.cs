#if G2
using Grasshopper2.Components;
using System.Collections;

namespace OneCodeTwoVersions.Polyfill;
public abstract partial class ComponentMiddleware<T> where T : ComponentMiddleware<T>
{
    private sealed class DataAccessWrapper(ComponentMiddleware<T> comp) : IGH_DataAccess
    {
        private readonly ComponentMiddleware<T> _comp = comp;
        private IDataAccess Access => _comp._currentAccess!;
        public int Iteration => Access.Iterations;

        public void AbortComponentSolution()
        {
            _comp.Document.Solution.Stop();
        }

        public bool GetData<T1>(int index, ref T1 destination)
        {
            throw new NotImplementedException();
        }

        public bool GetData<T1>(string name, ref T1 destination)
        {
            throw new NotImplementedException();
        }

        public bool GetDataList<T1>(int index, List<T1> list)
        {
            throw new NotImplementedException();
        }

        public bool GetDataList<T1>(string name, List<T1> list)
        {
            throw new NotImplementedException();
        }

        public bool SetData(int paramIndex, object data)
        {
            throw new NotImplementedException();
        }

        public bool SetData(int paramIndex, object data, int itemIndexOverride)
        {
            throw new NotImplementedException();
        }

        public bool SetData(string paramName, object data)
        {
            throw new NotImplementedException();
        }

        public bool SetDataList(int paramIndex, IEnumerable data)
        {
            throw new NotImplementedException();
        }

        public bool SetDataList(int paramIndex, IEnumerable data, int listIndexOverride)
        {
            throw new NotImplementedException();
        }

        public bool SetDataList(string paramName, IEnumerable data)
        {
            throw new NotImplementedException();
        }
    }
}

#endif