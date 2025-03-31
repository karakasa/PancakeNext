using PancakeNextCore.Dataset;
using PancakeNextCore.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PancakeNextCore.DataTypes;

public abstract class Quantity : IEquatable<Quantity>, IComparable<Quantity>
{
    public abstract string UnitName { get; }
    public abstract QuantityType UnitType { get; }
    public abstract double ToNeutralUnit();
    public abstract void FromNeutralUnit(double neutralAmount);
    public abstract double ToDocumentUnit();
    public abstract void FromDocumentUnit(double quantity);
    public abstract bool IsNegative { get; }
    public virtual Quantity DuplicateAndNegate()
    {
        throw new InvalidOperationException("The quantity cannot be negative.");
    }
    public virtual bool SupportDocumentUnitConversion => false;
    public static bool TryParseString(string str, [NotNullWhen(true)] out Quantity? qty)
    {
        if (DecimalLength.TryParse(str, out var qty1))
        {
            qty = qty1;
            return true;
        }

        if (FeetInchLength.TryParse(str, out var qty2))
        {
            qty = qty2;
            return true;
        }

        qty = default;
        return false;
    }

    public abstract Quantity Duplicate();
    
    public Quantity ConvertToUnit(string unit)
    {
        if (unit.StartsWith("ftin", StringComparison.OrdinalIgnoreCase))
        {
            var precision = 64;
            var index = unit.IndexOf("@");
            if (index != -1)
                precision = Convert.ToInt32(unit.Substring(index + 1));

            var len = new FeetInchLength(ToNeutralUnit(), precision);
            return len;
        }
        else
        {
            if (!DecimalLengthInfo.TryDetermineUnit(unit, out var internalUnit))
                throw new ArgumentOutOfRangeException(nameof(unit));

            var len = new DecimalLength(internalUnit);
            len.FromNeutralUnit(ToNeutralUnit());
            if (this is DecimalLength dec)
            {
                len.Precision = dec.Precision;
            }

            return len;
        }
    }
    public Quantity ConvertToDecimalUnit(DecimalLengthInfo.DecimalUnit internalUnit)
    {
        var len = new DecimalLength(internalUnit);
        len.FromNeutralUnit(ToNeutralUnit());
        if (this is DecimalLength dec)
        {
            len.Precision = dec.Precision;
        }

        return len;
    }

    public static Quantity operator +(Quantity a, Quantity b)
    {
        a.ThrowWhenIncompatible(b);

        var c = a.Duplicate();
        c.FromNeutralUnit(a.ToNeutralUnit() + b.ToNeutralUnit());
        return c;
    }

    public static Quantity operator -(Quantity a)
    {
        return a.DuplicateAndNegate();
    }

    public static Quantity operator -(Quantity a, Quantity b)
    {
        a.ThrowWhenIncompatible(b);

        var c = a.Duplicate();
        c.FromNeutralUnit(a.ToNeutralUnit() - b.ToNeutralUnit());
        return c;
    }

    public static Quantity operator *(Quantity a, int b)
    {
        var c = a.Duplicate();
        c.FromNeutralUnit(a.ToNeutralUnit() * b);
        return c;
    }

    public static Quantity operator *(Quantity a, double b)
    {
        var c = a.Duplicate();
        c.FromNeutralUnit(a.ToNeutralUnit() * b);
        return c;
    }

    public static Quantity operator *(int b, Quantity a)
    {
        return a * b;
    }

    public static Quantity operator *(double b, Quantity a)
    {
        return a * b;
    }

    public static Quantity operator /(Quantity a, int b)
    {
        return a * (1.0 / b);
    }

    public static Quantity operator /(Quantity a, double b)
    {
        return a * (1.0 / b);
    }

    public static double operator /(Quantity a, Quantity b)
    {
        a.ThrowWhenIncompatible(b);

        return a.ToNeutralUnit() / b.ToNeutralUnit();
    }

    public static Quantity operator %(Quantity a, Quantity b)
    {
        a.ThrowWhenIncompatible(b);

        var c = a.Duplicate();
        c.FromNeutralUnit(a.ToNeutralUnit() % b.ToNeutralUnit());
        return c;
    }

    public static bool operator >(Quantity a, Quantity b)
    {
        a.ThrowWhenIncompatible(b);
        return a.ToNeutralUnit() > b.ToNeutralUnit();
    }

    public static bool operator <(Quantity a, Quantity b)
    {
        a.ThrowWhenIncompatible(b);
        return a.ToNeutralUnit() < b.ToNeutralUnit();
    }

    public static bool operator ==(Quantity? a, Quantity? b)
    {
        if (a is null) return b is null;
        if (b is null) return a is null;

        a.ThrowWhenIncompatible(b);
        return a.ToNeutralUnit() == b.ToNeutralUnit();
    }

    public static bool operator !=(Quantity? a, Quantity? b)
    {
        return !(a == b);
    }

    public static bool operator >=(Quantity a, Quantity b)
    {
        a.ThrowWhenIncompatible(b);
        return a.ToNeutralUnit() >= b.ToNeutralUnit();
    }

    public static bool operator <=(Quantity a, Quantity b)
    {
        a.ThrowWhenIncompatible(b);
        return a.ToNeutralUnit() <= b.ToNeutralUnit();
    }

    private bool IsCompatibleWith(Quantity another)
    {
        return UnitType == another.UnitType;
    }

    internal void ThrowWhenIncompatible(Quantity another)
    {
        if (!IsCompatibleWith(another))
            ThrowIncompatible(UnitType, another.UnitType);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowIncompatible(QuantityType thisType, QuantityType anotherType)
    {
        throw new InvalidOperationException($"{thisType} cannot be operated with {anotherType}");
    }

    public override bool Equals(object? obj)
    {
        return obj is Quantity other && Equals(other);
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public bool Equals(Quantity? other)
    {
        if (other is null) return false;
        return this.UnitType == other.UnitType && 
            this.UnitName == other.UnitName &&
            this.ToNeutralUnit() == other.ToNeutralUnit();
    }

    public int CompareTo(Quantity? other)
    {
        if (other is null) return 1;
        ThrowWhenIncompatible(other);

        return ToNeutralUnit().CompareTo(other.ToNeutralUnit());
    }
}
