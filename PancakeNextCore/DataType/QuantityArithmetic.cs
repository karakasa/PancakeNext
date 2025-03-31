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
    public static Quantity Add(Quantity a, Quantity b)
    {
        return a + b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Sub)]
    public static Quantity Sub(Quantity a, Quantity b)
    {
        return a - b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Mul)]
    public static Quantity Mul(Quantity a, int b)
    {
        return a * b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Mul)]
    public static Quantity Mul(Quantity a, double b)
    {
        return a * b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Mul)]
    public static Quantity Mul(int a, Quantity b)
    {
        return a * b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Mul)]
    public static Quantity Mul(double a, Quantity b)
    {
        return a * b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Div)]
    public static Quantity Div(Quantity a, int b)
    {
        return a / b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Div)]
    public static Quantity Div(Quantity a, double b)
    {
        return a / b;
    }
    [Arithmetic.Operator(Arithmetic.Operation.Div)]
    public static double Div(Quantity a, Quantity b)
    {
        return a / b;
    }
    [Arithmetic.Function(Arithmetic.Binary.Max)]
    public static Quantity Max(Quantity a, Quantity b)
    {
        return a > b ? a : b;
    }
    [Arithmetic.Function(Arithmetic.Binary.Min)]
    public static Quantity Min(Quantity a, Quantity b)
    {
        return a < b ? a : b;
    }
    [Arithmetic.Function(Arithmetic.Ternary.Clip)]
    public static Quantity Clip(Quantity v, Quantity min, Quantity max)
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
    public static Quantity Mod(Quantity a, Quantity b)
    {
        return a % b;
    }

    [Arithmetic.Function(Arithmetic.Binary.Multiple)]
    public static Quantity Multiple(Quantity a, Quantity b)
    {
        b = Abs(b);
        var retVal = a.Duplicate();
        retVal.FromNeutralUnit((Math.Truncate(a / b) * b).ToNeutralUnit());

        if (a is FeetInchLength && b is FeetInchLength && retVal is FeetInchLength fi)
        {
            const double FeetInchError = 0.0001;
            fi.SetPreciseWithinError(FeetInchError);
        }

        return retVal;
    }

    [Arithmetic.Function(Arithmetic.Binary.Round)]
    public static Quantity Round(Quantity a, int b)
    {
        if (a is DecimalLength dv)
        {
            var q = (DecimalLength)dv.Duplicate();
            q.RawValue = Math.Round(q.RawValue, b);
            return q;
        }
        else if (a is FeetInchLength fi)
        {
            if (b <= 0)
            {
                throw new ArgumentException("Round value must be larger than 1.");
            }

            return new FeetInchLength(fi.ToNeutralUnit(), b);
        }

        throw new NotSupportedException();
    }

    [Arithmetic.Function(Arithmetic.Unary.Abs)]
    public static Quantity Abs(Quantity a) => a.IsNegative ? -a : a;

    [Arithmetic.Function(Arithmetic.Unary.Sign)]
    public static int Sign(Quantity a)
    {
        return Math.Sign(a.GetRawValue());
    }

    [Arithmetic.Function(Arithmetic.Unary.Truncate)]
    public static Quantity Truncate(Quantity a)
    {
        if (a is DecimalLength dv)
        {
            var q = (DecimalLength)dv.Duplicate();
            q.RawValue = Math.Truncate(q.RawValue);
            return q;
        }
        else if (a is FeetInchLength fi)
        {
            return fi.GetWholeAndRemainderPart().Whole;
        }

        throw new NotSupportedException();
    }

    [Arithmetic.Function(Arithmetic.Unary.Remainder)]
    public static Quantity Remainder(Quantity a)
    {
        if (a is DecimalLength dv)
        {
            var q = (DecimalLength)dv.Duplicate();
            q.RawValue = GetRemainderPart(q.RawValue);
            return q;
        }
        else if (a is FeetInchLength fi)
        {
            return fi.GetWholeAndRemainderPart().Remainder;
        }

        throw new NotSupportedException();
    }

    private static FeetInchLength GetOneTinyInch(FeetInchLength fi)
    {
        var retVal = new FeetInchLength(0, 1, 0, 0, precision: fi.Precision);
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
    public static Quantity Floor(Quantity a)
    {
        if (a is DecimalLength dv)
        {
            var q = (DecimalLength)dv.Duplicate();
            q.RawValue = Math.Floor(q.RawValue);
            return q;
        }
        else if (a is FeetInchLength fi)
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
    public static Quantity Ceiling(Quantity a)
    {
        if (a is DecimalLength dv)
        {
            var q = (DecimalLength)dv.Duplicate();
            q.RawValue = Math.Ceiling(q.RawValue);
            return q;
        }
        else if (a is FeetInchLength fi)
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
