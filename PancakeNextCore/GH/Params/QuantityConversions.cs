using Grasshopper2.Types;
using Grasshopper2.Types.Conversion;
using PancakeNextCore.Dataset;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
public sealed class QuantityConversions : ConversionRepository
{
    public static Merit StringToQuantity(string str, out GhQuantity? qty, out string message)
    {
        if (!GhQuantity.TryParseString(str, out qty))
        {
            message = "Unknown format.";
            return Merit.Zilch;
        }

        message = "";
        return Merit.Direct;
    }

    public static Merit QuantityToAmount(GhQuantity qty, out double amt, out string message)
    {
        var rhinoUnit = RhinoDocServer.ModelUnitSystem;
        if (!GhDecimalLengthInfo.TryConvertToRhinoUnit(qty.ToNeutralUnit(), rhinoUnit, out amt))
        {
            message = $"Unknown unit {rhinoUnit}";
            amt = 0.0;
            return Merit.Zilch;
        }

        message = "";
        return Merit.Fair;
    }

    public static Merit AmountToQuantity(double amt, out GhQuantity? qty, out string message)
    {
        var rhinoUnit = RhinoDocServer.ModelUnitSystem;
        if (!GhDecimalLengthInfo.TryDetermineUnit(rhinoUnit, out var pancakeUnit))
        {
            message = $"Unknown unit {rhinoUnit}";
            qty = null;
            return Merit.Zilch;
        }

        qty = new GhLengthDecimal(amt, pancakeUnit);
        message = "";
        return Merit.Fair;
    }
}
