using Grasshopper2.Types;
using Grasshopper2.Types.Conversion;
using PancakeNextCore.Dataset;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataTypes;
public sealed class QuantityConversions : ConversionRepository
{
    public static Merit StringToQuantity(string str, out Quantity? qty, out string message)
    {
        if (!Quantity.TryParseString(str, out qty))
        {
            message = "Unknown format.";
            return Merit.Zilch;
        }

        message = "";
        return Merit.Direct;
    }

    public static Merit QuantityToAmount(Quantity qty, out double amt, out string message)
    {
        var rhinoUnit = RhinoDocServer.ModelUnitSystem;
        if (!DecimalLengthInfo.TryConvertToRhinoUnit(qty.ToNeutralUnit(), rhinoUnit, out amt))
        {
            message = $"Unknown unit {rhinoUnit}";
            amt = 0.0;
            return Merit.Zilch;
        }

        message = "";
        return Merit.Fair;
    }

    public static Merit AmountToQuantity(double amt, out Quantity? qty, out string message)
    {
        var rhinoUnit = RhinoDocServer.ModelUnitSystem;
        if (!DecimalLengthInfo.TryDetermineUnit(rhinoUnit, out var pancakeUnit))
        {
            message = $"Unknown unit {rhinoUnit}";
            qty = null;
            return Merit.Zilch;
        }

        qty = new DecimalLength(amt, pancakeUnit);
        message = "";
        return Merit.Fair;
    }
}
