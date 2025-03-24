using Grasshopper2.Data;
using Grasshopper2.Types;
using Grasshopper2.Types.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataTypes;
public sealed class AssociationConversions : ConversionRepository
{
    public static Merit AssocToNTuple(Association assoc, out NTuple? tuple, out string message)
    {
        if (assoc.Length == 0)
        {
            tuple = NTuple.Empty;
            message = "";
            return Merit.Direct;
        }

        tuple = null;

        if (assoc is not NamedAssociation kv)
        {
            message = "Only named Association can be converted.";
            return Merit.Zilch;
        }

        tuple = new NTuple();

        foreach (var it in kv.GetPairs())
        {
            var name = it.Key;
            if (name is null)
            {
                message = "Association may not contain anonymous members.";
                return Merit.Zilch;
            }

            if (!NTuple.IsValidName(name, out var validName, out var reasons))
            {
                message = $"{name} is not a valid name. Possible alternative: {validName}.";
                return Merit.Zilch;
            }

            tuple.Grow(name, Garden.Pear(it.Value));
        }

        message = "";
        return Merit.Direct;
    }

    public static Merit NTupleToAssoc(NTuple tuple, out Association? assoc, out string message)
    {
        var n = new NamedAssociation();

        foreach (var (name, pear) in tuple.Elements)
        {
            n.Add(name, pear.Item);
        }

        assoc = n;
        message = "";
        return Merit.Direct;
    }
}