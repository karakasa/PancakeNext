using Grasshopper2.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;

internal static class GH_ParamAccess
{
    [Obsolete]
    public static readonly Access item = Access.Item;
    [Obsolete]

    public static readonly Access list = Access.Twig;
    [Obsolete]

    public static readonly Access tree = Access.Tree;
}
