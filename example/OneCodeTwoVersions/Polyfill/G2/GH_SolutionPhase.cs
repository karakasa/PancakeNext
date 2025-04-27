using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public enum GH_SolutionPhase : byte
{
    Blank = 0,
    Collecting,
    Collected,
    Computing,
    Computed,
    Failed = 10
}
