#if G2
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract class GH_ParamManager
{
    /// <summary>
    /// Gets the total number of parameters.
    /// </summary>
    public abstract int ParamCount { get; }

    /// <summary>
    /// Gets the parameter at the given index.
    /// </summary>
    public abstract IGH_Param this[int index] { get; }
}
#endif