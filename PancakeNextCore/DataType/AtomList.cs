using GrasshopperIO;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataType;
[IoId("22B79783-C674-4BC4-AFBA-014C94D727BE")]
public sealed class AtomList : Association
{
    public AtomList()
    {
    }
    public AtomList(IReader reader) : base(reader)
    {
    }

    public AtomList(params object?[] objects)
    {
        EnsureData(objects.Length);
        Values.AddRange(objects);
    }

    internal override Association GenericClone()
    {
        return new AtomList()
        {
            Values = HasValues ? new(Values) : null
        };
    }

    public void Add(object? obj)
    {
        EnsureData(0);
        Values.Add(obj);
    }

    public override IEnumerable<string> GetNamesForExport()
    {
        if (!HasValues) yield break;

        for(var i = 0; i < Values.Count; i++)
        {
            yield return i.ToString(CultureInfo.InvariantCulture);
        }
    }

    internal IEnumerable<object?> GetFlattenInnerListUnwrapped()
    {
        if (!HasValues) yield break;

        foreach (var it in Values)
        {
            if (it is AtomList list)
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
