using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Types.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rhino.Runtime.ViewCaptureWriter;

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

    public static IParameter With(this IParameter p, Requirement req)
    {
        p.Requirement = req;
        return p;
    }

    public static double? GetDuration(this ActiveObject obj)
    {
        return obj.State?.Data?.Duration.TotalMilliseconds;
    }

    public static Tree<T> MimicTreeWithOneValue<T>(ITree source, T value, bool copyMeta = false)
    {
        var newTwigs = source.AllTwigs.Select(x =>
        {
            var meta = (copyMeta && x.MetaCount != 0) ? x.ToMetaArray(ToArrayMethod.Always) : null;
            return Garden.TwigFromList(Enumerable.Repeat(value, x.LeafCount), meta);
        });

        return Garden.TreeFromTwigs(source.Paths, newTwigs);
    }
    public static Tree<T> MimicTreeWithValueNulls<T>(ITree source, bool copyMeta = false) where T : struct
    {
        var newTwigs = source.AllTwigs.Select(x =>
        {
            var meta = (copyMeta && x.MetaCount != 0) ? x.ToMetaArray(ToArrayMethod.Always) : null;
            return Garden.TwigFromList(Enumerable.Repeat(default(T), x.LeafCount), meta, Enumerable.Repeat(true, x.LeafCount));
        });

        return Garden.TreeFromTwigs(source.Paths, newTwigs);
    }
}
