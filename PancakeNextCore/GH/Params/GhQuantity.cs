using PancakeNextCore.Dataset;
using PancakeNextCore.Utility;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PancakeNextCore.GH.Params;

public abstract class GhQuantity : IEquatable<GhQuantity>, IComparable<GhQuantity>
{
    public abstract string UnitName { get; }
    public abstract QuantityType UnitType { get; }
    public abstract double ToNeutralUnit();
    public abstract void FromNeutralUnit(double neutralAmount);
    public abstract double ToDocumentUnit();
    public abstract void FromDocumentUnit(double quantity);
    public abstract bool IsNegative { get; }
    internal abstract double GetRawValue();
    public virtual GhQuantity DuplicateAndNegate()
    {
        throw new InvalidOperationException("The quantity cannot be negative.");
    }
    public virtual bool SupportDocumentUnitConversion => false;
    public static bool TryParseString(string str, [NotNullWhen(true)] out GhQuantity? qty)
    {
        if (GhLengthDecimal.TryParse(str, out var qty1))
        {
            qty = qty1;
            return true;
        }

        if (GhLengthFeetInch.TryParse(str, out var qty2))
        {
            qty = qty2;
            return true;
        }

        qty = default;
        return false;
    }

    public abstract GhQuantity Duplicate();

    public GhQuantity ConvertToUnit(string unit)
    {
        if (unit.StartsWith("ftin", StringComparison.OrdinalIgnoreCase))
        {
            var precision = 64;
            var index = unit.IndexOf("@");
            if (index != -1)
                precision = Convert.ToInt32(unit.Substring(index + 1));

            var len = new GhLengthFeetInch(ToNeutralUnit(), precision);
            return len;
        }
        else
        {
            if (!GhDecimalLengthInfo.TryDetermineUnit(unit, out var internalUnit))
                throw new ArgumentOutOfRangeException(nameof(unit));

            var len = new GhLengthDecimal(internalUnit);
            len.FromNeutralUnit(ToNeutralUnit());
            if (this is GhLengthDecimal dec)
            {
                len.Precision = dec.Precision;
            }

            return len;
        }
    }
    public GhQuantity ConvertToDecimalUnit(GhDecimalLengthInfo.DecimalUnit internalUnit)
    {
        var len = new GhLengthDecimal(internalUnit);
        len.FromNeutralUnit(ToNeutralUnit());
        if (this is GhLengthDecimal dec)
        {
            len.Precision = dec.Precision;
        }

        return len;
    }

    public static GhQuantity operator +(GhQuantity a, GhQuantity b)
    {
        a.ThrowWhenIncompatible(b);

        var c = a.Duplicate();
        c.FromNeutralUnit(a.ToNeutralUnit() + b.ToNeutralUnit());
        return c;
    }

    public static GhQuantity operator -(GhQuantity a)
    {
        return a.DuplicateAndNegate();
    }

    public static GhQuantity operator -(GhQuantity a, GhQuantity b)
    {
        a.ThrowWhenIncompatible(b);

        var c = a.Duplicate();
        c.FromNeutralUnit(a.ToNeutralUnit() - b.ToNeutralUnit());
        return c;
    }

    public static GhQuantity operator *(GhQuantity a, int b)
    {
        var c = a.Duplicate();
        c.FromNeutralUnit(a.ToNeutralUnit() * b);
        return c;
    }

    public static GhQuantity operator *(GhQuantity a, double b)
    {
        var c = a.Duplicate();
        c.FromNeutralUnit(a.ToNeutralUnit() * b);
        return c;
    }

    public static GhQuantity operator *(int b, GhQuantity a)
    {
        return a * b;
    }

    public static GhQuantity operator *(double b, GhQuantity a)
    {
        return a * b;
    }

    public static GhQuantity operator /(GhQuantity a, int b)
    {
        return a * (1.0 / b);
    }

    public static GhQuantity operator /(GhQuantity a, double b)
    {
        return a * (1.0 / b);
    }

    public static double operator /(GhQuantity a, GhQuantity b)
    {
        a.ThrowWhenIncompatible(b);

        return a.ToNeutralUnit() / b.ToNeutralUnit();
    }

    public static GhQuantity operator %(GhQuantity a, GhQuantity b)
    {
        a.ThrowWhenIncompatible(b);

        var c = a.Duplicate();
        c.FromNeutralUnit(a.ToNeutralUnit() % b.ToNeutralUnit());
        return c;
    }

    public static bool operator >(GhQuantity a, GhQuantity b)
    {
        a.ThrowWhenIncompatible(b);
        return a.ToNeutralUnit() > b.ToNeutralUnit();
    }

    public static bool operator <(GhQuantity a, GhQuantity b)
    {
        a.ThrowWhenIncompatible(b);
        return a.ToNeutralUnit() < b.ToNeutralUnit();
    }

    public static bool operator ==(GhQuantity? a, GhQuantity? b)
    {
        if (a is null) return b is null;
        if (b is null) return a is null;

        a.ThrowWhenIncompatible(b);
        return a.ToNeutralUnit() == b.ToNeutralUnit();
    }

    public static bool operator !=(GhQuantity? a, GhQuantity? b)
    {
        return !(a == b);
    }

    public static bool operator >=(GhQuantity a, GhQuantity b)
    {
        a.ThrowWhenIncompatible(b);
        return a.ToNeutralUnit() >= b.ToNeutralUnit();
    }

    public static bool operator <=(GhQuantity a, GhQuantity b)
    {
        a.ThrowWhenIncompatible(b);
        return a.ToNeutralUnit() <= b.ToNeutralUnit();
    }

    private bool IsCompatibleWith(GhQuantity another)
    {
        return UnitType == another.UnitType;
    }

    internal void ThrowWhenIncompatible(GhQuantity another)
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
        return obj is GhQuantity other && Equals(other);
    }

    public override int GetHashCode()
    {
        throw new UnreachableException();
    }

    public bool Equals(GhQuantity? other)
    {
        if (other is null) return false;
        return UnitType == other.UnitType &&
            UnitName == other.UnitName &&
            ToNeutralUnit() == other.ToNeutralUnit();
    }

    public int CompareTo(GhQuantity? other)
    {
        if (other is null) return 1;
        ThrowWhenIncompatible(other);

        return ToNeutralUnit().CompareTo(other.ToNeutralUnit());
    }
}
