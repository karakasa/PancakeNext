#if G2
using Grasshopper2.Interop;
using Grasshopper2.Parameters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;

public interface IGH_Param : IGH_ActiveObject
{
    public IParameter UnderlyingObject { get; }
    GH_DataMapping DataMapping { get; set; }
    bool Reverse { get; set; }
    bool Simplify { get; set; }

    GH_ParamKind Kind { get; }

    Type Type { get; }

    string TypeName { get; }

    bool Optional { get; set; }

    GH_ParamAccess Access { get; }

    IList<IGH_Param> Sources { get; }

    int SourceCount { get; }

    IList<IGH_Param> Recipients { get; }

    int VolatileDataCount { get; }

    IGH_Structure VolatileData { get; }
	void RemoveEffects();
    void AddSource(IGH_Param source);
    void AddSource(IGH_Param source, int index);
    void RemoveSource(IGH_Param source);
    void RemoveSource(Guid source_id);
    void RemoveAllSources();
    void ReplaceSource(IGH_Param old_source, IGH_Param new_source);
    void ReplaceSource(Guid old_source_id, IGH_Param new_source);
}

#endif