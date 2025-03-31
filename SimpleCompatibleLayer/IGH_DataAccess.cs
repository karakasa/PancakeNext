using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompatibleLayer;
public interface IGH_DataAccess
{

    int Iteration { get; }

    void IncrementIteration();

    void DisableGapLogic();

    void DisableGapLogic(int paramIndex);

    GH_Path ParameterTargetPath(int paramIndex);

    int ParameterTargetIndex(int paramIndex);

    void AbortComponentSolution();

    List<T> Util_RemoveNullRefs<T>(List<T> L);

    int Util_CountNullRefs<T>(List<T> L);

    int Util_CountNonNullRefs<T>(List<T> L);

    bool Util_EnsureNonNullCount<T>(List<T> L, int N);

    int Util_FirstNonNullItem<T>(List<T> L);

    bool SetData(int paramIndex, object data);

    bool SetData(int paramIndex, object data, int itemIndexOverride);

    bool SetData(string paramName, object data);

    bool SetDataList(int paramIndex, IEnumerable data);

    bool SetDataList(int paramIndex, IEnumerable data, int listIndexOverride);

    bool SetDataList(string paramName, IEnumerable data);

    bool SetDataTree(int paramIndex, IGH_DataTree tree);

    bool SetDataTree(int paramIndex, IGH_Structure tree);

    bool BlitData<Q>(int paramIndex, GH_Structure<Q> tree, bool overwrite) where Q : IGH_Goo;

    bool GetData<T>(int index, ref T destination);

    bool GetData<T>(string name, ref T destination);

    bool GetDataList<T>(int index, List<T> list);

    bool GetDataList<T>(string name, List<T> list);

    bool GetDataTree<T>(int index, out GH_Structure<T> tree) where T : IGH_Goo;

    bool GetDataTree<T>(string name, out GH_Structure<T> tree) where T : IGH_Goo;
}