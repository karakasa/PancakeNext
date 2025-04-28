using Grasshopper2.Types;
using Grasshopper2.Types.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper2.Data;
using Path = Grasshopper2.Data.Path;

namespace OneCodeTwoVersions.Polyfill;
public sealed class PathConversion : ConversionRepository
{
    public static Merit Path1To2(GH_Path path, out Path? tuple, out string message)
    {
        tuple = path.To2();
        message = "";
        return Merit.Direct;
    }

    public static Merit Path2To1(Path path, out GH_Path? tuple, out string message)
    {
        tuple = path.To1();
        message = "";
        return Merit.Direct;
    }
}
