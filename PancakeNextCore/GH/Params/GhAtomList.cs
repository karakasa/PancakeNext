using Grasshopper2.Data;
using GrasshopperIO;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
[IoId("22B79783-C674-4BC4-AFBA-014C94D727BE")]
public sealed class GhAtomList : GhAssocBase
{
    public GhAtomList()
    {
    }
    public GhAtomList(IReader reader) : base(reader)
    {
    }

    public GhAtomList(object?[] objects)
    {
        EnsureData(objects.Length);
        Values.AddRange(objects.AsPears());
    }

    public GhAtomList(ITwig twig)
    {
        EnsureData(twig.ItemCount);
        Values.AddRange(twig.Pears);
    }

    public GhAtomList(IPear?[] pears)
    {
        EnsureData(pears.Length);
        Values.AddRange(pears);
    }

    internal override GhAssocBase GenericClone()
    {
        return new GhAtomList()
        {
            Values = HasValues ? new(Values) : null
        };
    }

    public void Add(object? obj)
    {
        Add(obj.AsPear());
    }

    public void Add(IPear? obj)
    {
        EnsureData(0);
        Values.Add(obj);
    }

    public override IEnumerable<string> GetNamesForExport()
    {
        if (!HasValues) yield break;

        for (var i = 0; i < Values.Count; i++)
        {
            yield return i.ToString(CultureInfo.InvariantCulture);
        }
    }

    internal IEnumerable<object?> GetFlattenInnerListUnwrapped()
    {
        if (!HasValues) yield break;

        foreach (var pear in Values)
        {
            var it = pear?.Item;

            if (it is GhAtomList list)
            {
                foreach (var it2 in list.GetFlattenInnerListUnwrapped())
                    yield return it2;
            }
            else
            {
                yield return it;
            }
        }
    }
}
