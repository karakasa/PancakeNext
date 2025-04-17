using Grasshopper2.Components;
using Grasshopper2.Components.Standard;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal static class DocumentTraverser
{
    public static IEnumerable<IParameter> AllParameters(this Document doc, bool onlyIndependent)
    {
        var parameters = doc.Objects.ActiveObjects
            .RecursivelyFlatten(static o => TryGetInnerObjects(o, treatBundleAsOne: true))
            .HarvestParameters(true, false);

        foreach (var param in parameters)
        {
            if (onlyIndependent && param.Inputs.Count > 0) continue;
            yield return param;
        }
    }

    public static IEnumerable<IDocumentObject> AllActiveObjects(this Document doc)
    {
        return doc.Objects.ActiveObjects
            .RecursivelyFlatten(static o => TryGetInnerObjects(o, treatBundleAsOne: false));
    }

    private static IEnumerable<IParameter> HarvestParameters(this IEnumerable<IDocumentObject> objs, bool compInput, bool compOutput)
    {
        foreach (var obj in objs)
        {
            if (obj is IParameter param)
            {
                yield return param;
            }
            else if (obj is Component comp)
            {
                if (compInput)
                    foreach (var inp in comp.Parameters.Inputs)
                        yield return inp;

                if (compOutput)
                    foreach (var output in comp.Parameters.Outputs)
                        yield return output;
            }
        }
    }

    private static IEnumerable<IDocumentObject>? TryGetInnerObjects(IDocumentObject obj, bool treatBundleAsOne)
    {
        return obj switch
        {
            Chain chain when !treatBundleAsOne => chain.Links,
            // TODO: Cluster cluster => XXXX,
            _ => null,
        };
    }
}
