using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public interface IGH_ActiveObject : IGH_DocumentObject
{
    // GH_SolutionPhase Phase { get; set; }
    bool Locked { get; set; }
}
