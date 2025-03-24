using GrasshopperIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataTypes;
[IoId("22B79783-C674-4BC4-AFBA-014C94D727BE")]
public sealed class AtomList : Association
{
    public AtomList()
    {
    }
    public AtomList(IReader reader) : base(reader)
    {
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
}
