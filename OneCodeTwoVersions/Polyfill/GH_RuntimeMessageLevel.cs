using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public enum GH_RuntimeMessageLevel : byte
{
    /// <summary>
    /// No warnings or errors were recorded during solver-time processes
    /// </summary>
    Blank = 0,
    /// <summary>
    /// One of more messages (but no warnings and no errors) were recorded during solver-time processes.
    /// </summary>
    Remark = byte.MaxValue,
    /// <summary>
    /// One or more warnings (but no errors) were recorded during solver-time processes
    /// </summary>
    Warning = 10,
    /// <summary>
    /// One or more errors (and possibly any number of warnings) were recorded during solver-time processes.
    /// </summary>
    Error = 20
}

