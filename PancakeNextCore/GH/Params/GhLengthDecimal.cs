using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PancakeNextCore.Dataset;
using PancakeNextCore.Polyfill;
using PancakeNextCore.Utility;
using Rhino;

namespace PancakeNextCore.DataType;

public sealed class GhLengthDecimal : GhQuantity
{
    public GhLengthDecimal() { }
    public override string UnitName => _currentUnit;

    public override QuantityType UnitType => QuantityType.Length;

    public int Precision = 4;

    private readonly string _currentUnit = "";

    private bool _isSet = false;

    public double RawValue { get; set; }
    public bool KeepDecimal { get; set; } = false;

    private readonly GhDecimalLengthInfo.DecimalUnit _unit;

    internal GhLengthDecimal(GhLengthDecimal metric)
    {
        RawValue = metric.RawValue;
        Precision = metric.Precision;
        _currentUnit = metric._currentUnit;
        _unit = metric._unit;

        _isSet = metric._isSet;
        KeepDecimal = metric.KeepDecimal;
    }

    private static void FindUnit(string symbol, out GhDecimalLengthInfo.DecimalUnit unit)
    {
        if (!GhDecimalLengthInfo.UnitList.TryGetValue(symbol, out unit))
            throw new ArgumentOutOfRangeException(nameof(symbol));
    }

    public GhLengthDecimal(double amount, string symbol, int precision = 4)
    {
        RawValue = amount;
        _currentUnit = symbol;
        Precision = precision;

        FindUnit(symbol, out _unit);

        _isSet = true;
    }

    public GhLengthDecimal(double amount, GhDecimalLengthInfo.DecimalUnit unit, int precision = 4)
    {
        RawValue = amount;
        _currentUnit = unit.Symbol;
        Precision = precision;
        _unit = unit;

        _isSet = true;
    }

    public GhLengthDecimal(GhDecimalLengthInfo.DecimalUnit unit, int precision = 4)
    {
        _currentUnit = unit.Symbol;
        Precision = precision;

        _unit = unit;

        _isSet = false;
    }

    public GhLengthDecimal(string symbol, int precision = 4)
    {
        _currentUnit = symbol;
        Precision = precision;

        FindUnit(symbol, out _unit);

        _isSet = false;
    }
    public override void FromNeutralUnit(double neutralAmount)
    {
        RawValue = neutralAmount / _unit.RatioToNeutral;
        _isSet = true;
    }

    public override double ToNeutralUnit()
    {
        return RawValue * _unit.RatioToNeutral;
    }

    public override string ToString()
    {
        if (Precision == 0 || IsInteger())
        {
            return $"{Convert.ToInt32(RawValue)} {_currentUnit}";
        }
        else
        {
            return RawValue.ToString($"F{Precision}") + " " + _currentUnit;
        }
    }

    private bool IsInteger()
    {
        if (KeepDecimal) return false;
        return Math.Abs(RawValue - Math.Truncate(RawValue)) <= RhinoMath.ZeroTolerance;
    }

    public override double ToDocumentUnit()
    {
        return GhDecimalLengthInfo.ConvertToRhinoUnit(ToNeutralUnit(), RhinoDocServer.ModelUnitSystem);
    }

    public override void FromDocumentUnit(double quantity)
    {
        var unit = RhinoDocServer.ActiveDoc?.ModelUnitSystem ?? UnitSystem.None;

        FromNeutralUnit(GhDecimalLengthInfo.ConvertFromRhinoUnit(quantity, unit));
    }

    public static bool TryParse(string str, [NotNullWhen(true)] out GhLengthDecimal? quantity)
    {
        if (!GhDecimalLengthInfo.TryDetermineUnit(str, out var unit))
        {
            quantity = null;
            return false;
        }

        var numeric = str.Substr(0, str.Length - unit.Symbol.Length);
        if (!double.TryParse(numeric, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
        {
            quantity = null;
            return false;
        }

        quantity = new GhLengthDecimal(amount, unit);
        return true;
    }

    public override bool SupportDocumentUnitConversion => true;
    public override GhQuantity Duplicate() => new GhLengthDecimal(this);
    public override GhQuantity DuplicateAndNegate()
    {
        var obj = new GhLengthDecimal(this);
        obj.RawValue = -obj.RawValue;
        return obj;
    }

    public override int GetHashCode()
    {
        return unchecked(RawValue.GetHashCode() * 7 + _currentUnit.GetHashCode() * 13);
    }

    public override bool IsNegative => RawValue < 0;
    internal override double GetRawValue() => RawValue;
}
