using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
internal static class PolyfillExtensions
{
    public static GH_Path ToCurrent(this GH_Path path)
    {
        return path;
    }
}
