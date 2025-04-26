using Grasshopper2.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public interface IGH_Structure
{
    bool IsEmpty { get; }

    int PathCount { get; }

    int DataCount { get; }

    IList<GH_Path> Paths { get; }

    IList<IList> StructureProxy { get; }

    string TopologyDescription { get; }

    GH_Path get_Path(int index);

    IList get_Branch(int index);
    IList get_Branch(GH_Path path);

    bool PathExists(GH_Path path);

    void RemovePath(GH_Path path);

    void ReplacePath(GH_Path find, GH_Path replace);

    IGH_StructureEnumerator AllData(bool skipNulls);

    void Clear();

    void ClearData();

    void EnsureCapacity(int capacity);
    void Flatten(GH_Path path = null);

    string DataDescription(bool includeIndices, bool includePaths);
    ITree To2();
}

public interface IGH_StructureEnumerator : IEnumerable<IGH_Goo>
{
}
