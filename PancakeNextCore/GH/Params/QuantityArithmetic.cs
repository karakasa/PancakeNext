using Grasshopper2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataType;
public sealed class QuantityArithmetic : ArithmeticRepository
{
    [Arithmetic.Operator(Arithmetic.Operation.Add)]
    public static GhQuantity Add(GhQuantity a, GhQuantity b)
    {
        return a + b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Sub)]
    public static GhQuantity Sub(GhQuantity a, GhQuantity b)
    {
        return a - b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Mul)]
    public static GhQuantity Mul(GhQuantity a, int b)
    {
        return a * b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Mul)]
    public static GhQuantity Mul(GhQuantity a, double b)
    {
        return a * b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Mul)]
    public static GhQuantity Mul(int a, GhQuantity b)
    {
        return a * b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Mul)]
    public static GhQuantity Mul(double a, GhQuantity b)
    {
        return a * b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Div)]
    public static GhQuantity Div(GhQuantity a, int b)
    {
        return a / b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Div)]
    public static GhQuantity Div(GhQuantity a, double b)
    {
        return a / b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Div)]
    public static double Div(GhQuantity a, GhQuantity b)
    {
        return a / b;
    }
    [Arithmetic.Function(Arithmetic.Binary.Max)]
    public static GhQuantity Max(GhQuantity a, GhQuantity b)
    {
        return a > b ? a : b;
    }
    [Arithmetic.Function(Arithmetic.Binary.Min)]
    public static GhQuantity Min(GhQuantity a, GhQuantity b)
    {
        return a < b ? a : b;
    }
    [Arithmetic.Function(Arithmetic.Ternary.Clip)]
    public static GhQuantity Clip(GhQuantity v, GhQuantity min, GhQuantity max)
    {
        v.ThrowWhenIncompatible(min);
        v.ThrowWhenIncompatible(max);

        var neutralOfV = v.ToNeutralUnit();
        var neutralOfMin = min.ToNeutralUnit();
        var neutralOfMax = max.ToNeutralUnit();

        if (neutralOfMin > neutralOfMax)
            (neutralOfMin, neutralOfMax) = (neutralOfMax, neutralOfMin);

        if (neutralOfV < neutralOfMin)
        {
            var newObj = v.Duplicate();
            newObj.FromNeutralUnit(neutralOfMin);
            return newObj;
        }
        else if (neutralOfV > neutralOfMax)
        {
            var newObj = v.Duplicate();
            newObj.FromNeutralUnit(neutralOfMax);
            return newObj;
        }
        else
        {
            return v;
        }
    }

    [Arithmetic.Function(Arithmetic.Binary.Mod)]
    public static GhQuantity Mod(GhQuantity a, GhQuantity b)
    {
        return a % b;
    }

    [Arithmetic.Function(Arithmetic.Binary.Multiple)]
    public static GhQuantity Multiple(GhQuantity a, GhQuantity b)
    {
        b = Abs(b);
        var retVal = a.Duplicate();
        retVal.FromNeutralUnit((Math.Truncate(a / b) * b).ToNeutralUnit());

        if (a is GhLengthFeetInch && b is GhLengthFeetInch && retVal is GhLengthFeetInch fi)
        {
            const double FeetInchError = 0.0001;
            fi.SetPreciseWithinError(FeetInchError);
        }

        return retVal;
    }

    [Arithmetic.Function(Arithmetic.Binary.Round)]
    public static GhQuantity Round(GhQuantity a, int b)
    {
        if (a is GhLengthDecimal dv)
        {
            var q = (GhLengthDecimal)dv.Duplicate();
            q.RawValue = Math.Round(q.RawValue, b);
            return q;
        }
        else if (a is GhLengthFeetInch fi)
        {
            if (b <= 0)
            {
                throw new ArgumentException("Round value must be larger than 1.");
            }

            return new GhLengthFeetInch(fi.ToNeutralUnit(), b);
        }

        throw new NotSupportedException();
    }

    [Arithmetic.Function(Arithmetic.Unary.Abs)]
    public static GhQuantity Abs(GhQuantity a) => a.IsNegative ? -a : a;

    [Arithmetic.Function(Arithmetic.Unary.Sign)]
    public static int Sign(GhQuantity a)
    {
        return Math.Sign(a.GetRawValue());
    }

    [Arithmetic.Function(Arithmetic.Unary.Truncate)]
    public static GhQuantity Truncate(GhQuantity a)
    {
        if (a is GhLengthDecimal dv)
        {
            var q = (GhLengthDecimal)dv.Duplicate();
            q.RawValue = Math.Truncate(q.RawValue);
            return q;
        }
        else if (a is GhLengthFeetInch fi)
        {
            return fi.GetWholeAndRemainderPart().Whole;
        }

        throw new NotSupportedException();
    }

    [Arithmetic.Function(Arithmetic.Unary.Remainder)]
    public static GhQuantity Remainder(GhQuantity a)
    {
        if (a is GhLengthDecimal dv)
        {
            var q = (GhLengthDecimal)dv.Duplicate();
            q.RawValue = GetRemainderPart(q.RawValue);
            return q;
        }
        else if (a is GhLengthFeetInch fi)
        {
            return fi.GetWholeAndRemainderPart().Remainder;
        }

        throw new NotSupportedException();
    }

    private static GhLengthFeetInch GetOneTinyInch(GhLengthFeetInch fi)
    {
        var retVal = new GhLengthFeetInch(0, 1, 0, 0, precision: fi.Precision);
        return retVal;
    }

    private static double GetRemainderPart(double v)
    {
        var negative = v < 0;
        if (v < 0) v = -v;

        var integerPart = (int)Math.Truncate(v);
        var remainderPart = v - integerPart;
        return negative ? -remainderPart : remainderPart;
    }

    [Arithmetic.Function(Arithmetic.Unary.Floor)]
    public static GhQuantity Floor(GhQuantity a)
    {
        if (a is GhLengthDecimal dv)
        {
            var q = (GhLengthDecimal)dv.Duplicate();
            q.RawValue = Math.Floor(q.RawValue);
            return q;
        }
        else if (a is GhLengthFeetInch fi)
        {
            var (whole, remainder) = fi.GetWholeAndRemainderPart();

            if (remainder.RawValue == 0)
            {
                return fi;
            }
            else
            {
                return fi.IsNegative ? whole - GetOneTinyInch(fi) : whole;
            }
        }

        throw new NotSupportedException();
    }

    [Arithmetic.Function(Arithmetic.Unary.Ceiling)]
    public static GhQuantity Ceiling(GhQuantity a)
    {
        if (a is GhLengthDecimal dv)
        {
            var q = (GhLengthDecimal)dv.Duplicate();
            q.RawValue = Math.Ceiling(q.RawValue);
            return q;
        }
        else if (a is GhLengthFeetInch fi)
        {
            var (whole, remainder) = fi.GetWholeAndRemainderPart();

            if (remainder.RawValue == 0)
            {
                return fi;
            }
            else
            {
                return fi.IsNegative ? whole : whole + GetOneTinyInch(fi);
            }
        }

        throw new NotSupportedException();
    }
}
