using Grasshopper2.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
#if G2
internal static class PolyfillExtensions
{
    public static Access To2(this GH_ParamAccess access)
    {
        return access switch
        {
            GH_ParamAccess.item => Access.Item,
            GH_ParamAccess.list => Access.Twig,
            GH_ParamAccess.tree => Access.Tree,
            _ => throw new ArgumentOutOfRangeException(nameof(access), "Invalid GH_ParamAccess")
        };
    }
}
#endif