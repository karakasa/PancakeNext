using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
#if G2
public enum GH_ParamAccess
{
    item,
    list,
    tree
}
#endif
