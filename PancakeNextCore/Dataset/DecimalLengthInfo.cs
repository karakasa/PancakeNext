using Rhino;
using System;
using System.Collections.Generic;

namespace PancakeNextCore.Dataset;

public static class DecimalLengthInfo
{
    public struct DecimalUnit
    {
        public string Symbol;
        public double RatioToNeutral;
        public UnitSystem RhinoUnit;
    }

    public static readonly Dictionary<string, DecimalUnit> UnitList = [];

    private static readonly Dictionary<UnitSystem, double> RhinoRatio = [];

    static DecimalLengthInfo()
    {
        AddDecimalUnit("mm", 0.001, UnitSystem.Millimeters);
        AddDecimalUnit("cm", 0.01, UnitSystem.Centimeters);
        AddDecimalUnit("dm", 0.1, UnitSystem.Decimeters);
        AddDecimalUnit("km", 1000, UnitSystem.Kilometers);
        AddDecimalUnit("mile", 1609.34, UnitSystem.Miles);
        AddDecimalUnit("yard", 0.9144, UnitSystem.Yards);
        AddDecimalUnit("in", 0.0254, UnitSystem.Inches);
        AddDecimalUnit("ft", 0.3048, UnitSystem.Feet);

        AddDecimalUnit("m", 1, UnitSystem.Meters);

        AddRhinoRatio(UnitSystem.None, 1);
        AddRhinoRatio(UnitSystem.Angstroms, 1e-10);
        AddRhinoRatio(UnitSystem.Nanometers, 1e-9);
        AddRhinoRatio(UnitSystem.Microns, 1e-6);
        AddRhinoRatio(UnitSystem.Millimeters, 1e-3);
        AddRhinoRatio(UnitSystem.Centimeters, 1e-2);
        AddRhinoRatio(UnitSystem.Decimeters, 1e-1);
        AddRhinoRatio(UnitSystem.Meters, 1);
        AddRhinoRatio(UnitSystem.Dekameters, 1e1);
        AddRhinoRatio(UnitSystem.Hectometers, 1e2);
        AddRhinoRatio(UnitSystem.Kilometers, 1e3);
        AddRhinoRatio(UnitSystem.Megameters, 1e6);
        AddRhinoRatio(UnitSystem.Gigameters, 1e9);
        AddRhinoRatio(UnitSystem.Microinches, 2.54e-8);
        AddRhinoRatio(UnitSystem.Mils, 2.54e-5);
        AddRhinoRatio(UnitSystem.Inches, 0.0254);
        AddRhinoRatio(UnitSystem.Feet, 0.3048);
        AddRhinoRatio(UnitSystem.Yards, 0.9144);
        AddRhinoRatio(UnitSystem.Miles, 1609.344);
        AddRhinoRatio(UnitSystem.PrinterPoints, 0.0254 / 72); // Printer Points
        AddRhinoRatio(UnitSystem.PrinterPicas, 0.0254 / 6); // Printer Picas
        AddRhinoRatio(UnitSystem.NauticalMiles, 1852); // Nautical Miles
        AddRhinoRatio(UnitSystem.AstronomicalUnits, 1.4959787e+11); // Astronomical Units
        AddRhinoRatio(UnitSystem.LightYears, 9.4607304725808e+15); // Lightyear
        AddRhinoRatio(UnitSystem.Parsecs, 3.08567758e+16);
    }

    private static void AddRhinoRatio(UnitSystem unit, double ratio)
    {
        RhinoRatio.Add(unit, ratio);
    }

    private static void AddDecimalUnit(string symbol, double ratio, UnitSystem rhino = UnitSystem.None)
    {
        UnitList.Add(symbol, new DecimalUnit
        {
            Symbol = symbol,
            RatioToNeutral = ratio,
            RhinoUnit = rhino
        });
    }

    public static bool TryConvertToRhinoUnit(double meter, UnitSystem rhinoUnit, out double amt)
    {
        if (!RhinoRatio.TryGetValue(rhinoUnit, out var val))
        {
            amt = 0;
            return false;
        }
        amt = meter / val;
        return true;
    }

    public static double ConvertToRhinoUnit(double meter, UnitSystem rhinoUnit)
    {
        if (!RhinoRatio.TryGetValue(rhinoUnit, out var val))
            return meter;
        return meter / val;
    }

    public static double ConvertFromRhinoUnit(double meter, UnitSystem rhinoUnit)
    {
        if (!RhinoRatio.TryGetValue(rhinoUnit, out var val))
            return meter;
        return meter * val;
    }

    public static bool TryDetermineUnit(string str, out DecimalUnit unit)
    {
        foreach (var u in UnitList)
        {
            if (str.EndsWith(u.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                unit = u.Value;
                return true;
            }
        }

        unit = default;
        return false;
    }

    public static bool TryDetermineUnit(UnitSystem str, out DecimalUnit unit)
    {
        foreach (var u in UnitList)
        {
            if (u.Value.RhinoUnit == str)
            {
                unit = u.Value;
                return true;
            }
        }

        unit = default;
        return false;
    }

    public static DecimalUnit NeutralUnit { get => UnitList["m"]; }
}
