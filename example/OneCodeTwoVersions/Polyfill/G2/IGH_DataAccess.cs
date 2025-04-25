using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
#if G2
public interface IGH_DataAccess
{
	int Iteration { get; }
    void AbortComponentSolution();
    bool SetData(int paramIndex, object data);
    bool SetData(int paramIndex, object data, int itemIndexOverride);
    bool SetData(string paramName, object data);
    bool SetDataList(int paramIndex, IEnumerable data);

    bool SetDataList(int paramIndex, IEnumerable data, int listIndexOverride);

    bool SetDataList(string paramName, IEnumerable data);

    bool SetDataTree(int paramIndex, IGH_DataTree tree);

    bool SetDataTree(int paramIndex, IGH_Structure tree);
    bool GetData<T>(int index, ref T destination);
    bool GetData<T>(string name, ref T destination);

    bool GetDataList<T>(int index, List<T> list);

    bool GetDataList<T>(string name, List<T> list);

    bool GetDataTree<T>(int index, out GH_Structure<T> tree) where T : IGH_Goo;

    bool GetDataTree<T>(string name, out GH_Structure<T> tree) where T : IGH_Goo;
}
#endif