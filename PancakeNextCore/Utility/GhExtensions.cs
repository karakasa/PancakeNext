using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal static class GhExtensions
{
    public static bool TryGetReferenceId(this IPear pear, out Guid guid)
    {
        if (pear.HasMeta() && pear.Meta.HasReferenceId(out guid))
        {
            return true;
        }

        guid = Guid.Empty;
        return false;
    }
}
