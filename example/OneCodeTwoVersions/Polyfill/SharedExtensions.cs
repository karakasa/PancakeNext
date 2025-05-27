using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
internal static class SharedExtensions
{
    /// <summary>
    /// Force refresh of the component parameters in Grasshopper 2.
    /// Have no effect in Grasshopper 1.
    /// </summary>
    /// <param name="server"></param>
    public static void Refresh(this GH_ComponentParamServer server)
    {
#if G2
        server.InvalidateList();
#endif
    }
}
