using PancakeNextCore.Dataset;
using PancakeNextCore.Polyfill;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PancakeNextCore.DataType;

public sealed class GhLengthFeetInch : GhQuantity
{
    internal override double GetRawValue() => RawValue;
    public double RawValue { get; private set; }
    public double FeetLength => RawValue;

    public static int DefaultPrecision { get; set; } = 64;

    public override string UnitName => "ft";
    public override QuantityType UnitType => QuantityType.Length;
    public int FeetIntegerPart { get; private set; } = 0;
    public int InchIntegerPart { get; private set; } = 0;
    public int InchFractionPartFirst { get; private set; } = 0;
    public int InchFractionPartSecond { get; private set; } = 0;
    public double Error => Math.Abs(Math.Abs(RawValue) - (FeetIntegerPart + (InchIntegerPart
            + 1.0 * InchFractionPartFirst / InchFractionPartSecond) / 12));
    private bool Negative { get; set; } = false;

    public int Precision { get; private set; } = DefaultPrecision;
    public bool Precise { get; private set; } = false;
    internal void SetPreciseWithinError(double error) => Precise = Math.Abs(Error) < error;
    private void UpdateRawValue()
    {
        RawValue = (Negative ? -1 : 1) * (FeetIntegerPart + (InchIntegerPart
            + 1.0 * InchFractionPartFirst / InchFractionPartSecond) / 12);
    }

    public void UpdatePrecision(int newPrecision)
    {
        var oldPrecision = Precision;
        Precision = newPrecision;

        if (newPrecision < oldPrecision && !Precise)
        {
            FromFeetAmount(RawValue);
            return;
        }

        if (Precise)
        {
            if (newPrecision >= oldPrecision || newPrecision >= InchFractionPartSecond)
                return;

            FromFeetAmount(RawValue);
        }
    }

    private void FromFeetAmount(double amount)
    {
        Precise = false;
        Negative = false;

        var length = amount;
        if (length < 0)
        {
            length = -length;
            Negative = true;
        }
        RawValue = length;

        var feet = Convert.ToInt32(Math.Floor(length));
        FeetIntegerPart = feet;

        length -= feet;
        length *= 12;

        var inch = Convert.ToInt32(Math.Floor(length));
        InchIntegerPart = inch;

        length -= inch;

        var upper = Convert.ToInt32(Math.Round(length * Precision));
        var lower = Precision;
        var gcd = GetGreatestDivisor(upper, lower);

        InchFractionPartFirst = upper / gcd;
        InchFractionPartSecond = lower / gcd;

        if (InchFractionPartFirst == InchFractionPartSecond)
        {
            InchFractionPartFirst = 0;
            InchFractionPartSecond = 1;
            InchIntegerPart++;
        }

        if (InchIntegerPart >= 12)
        {
            FeetIntegerPart += InchIntegerPart / 12;
            InchIntegerPart %= 12;
        }

        if (FeetIntegerPart == 0 && InchIntegerPart == 0 && InchFractionPartFirst == 0)
            Negative = false;
    }

    private static int GetGreatestDivisor(int a, int b)
    {
        while (a != 0 && b != 0)
        {
            if (a > b)
            {
                a %= b;
            }
            else
            {
                b %= a;
            }
        }
        return b == 0 ? a : b;
    }

    public GhLengthFeetInch(int feet = 0, int inchInteger = 0, int inchFracFirst = 0,
        int inchFracSecond = 1, int precision = 64)
    {
        FeetIntegerPart = feet;
        InchIntegerPart = inchInteger;
        InchFractionPartFirst = inchFracFirst;
        InchFractionPartSecond = inchFracSecond;
        Precision = precision;
        Precise = true;

        if (FeetIntegerPart < 0)
        {
            FeetIntegerPart = -FeetIntegerPart;
            Negative = true;
        }

        NormalizeNumerics();

        if (inchFracSecond > Precision)
            Precision = inchFracSecond;

        UpdateRawValue();
    }

    public GhLengthFeetInch() : this(0)
    {

    }

    private static bool AllDigits(string str) => str.All(c => c >= '0' && c <= '9');

    internal GhLengthFeetInch(string feet, string inchInteger, string inchFracFirst,
        string inchFracSecond)
    {
        Precision = DefaultPrecision;

        if (AllDigits(feet) && AllDigits(inchInteger)
            && AllDigits(inchFracFirst) && AllDigits(inchFracSecond))
        {
            FeetIntegerPart = Convert.ToInt32(feet);
            InchIntegerPart = Convert.ToInt32(inchInteger);
            InchFractionPartFirst = Convert.ToInt32(inchFracFirst);
            InchFractionPartSecond = Convert.ToInt32(inchFracSecond);
            Precise = true;

            NormalizeNumerics();

            if (Precision < InchFractionPartSecond)
                Precision = InchFractionPartSecond;

            UpdateRawValue();
        }
        else
        {
            FromFeetAmount(Convert.ToDouble(feet) + (Convert.ToDouble(inchInteger)
                + Convert.ToDouble(inchFracFirst) / Convert.ToDouble(inchFracSecond)) / 12);
        }
    }

    private void NormalizeNumerics()
    {
        var gcd = GetGreatestDivisor(InchFractionPartFirst, InchFractionPartSecond);
        if (gcd != 1)
        {
            InchFractionPartFirst /= gcd;
            InchFractionPartSecond /= gcd;
        }

        if (InchFractionPartFirst >= InchFractionPartSecond)
        {
            var reminder = InchFractionPartFirst % InchFractionPartSecond;
            var added = InchFractionPartFirst / InchFractionPartSecond;

            InchFractionPartFirst = reminder;
            InchIntegerPart += added;
        }

        if (InchIntegerPart >= 12)
        {
            FeetIntegerPart += InchIntegerPart / 12;
            InchIntegerPart = InchIntegerPart % 12;
        }
    }

    public GhLengthFeetInch(double meter)
    {
        FromNeutralUnit(meter);
    }

    public GhLengthFeetInch(double meter, int precision)
    {
        Precision = precision;
        FromNeutralUnit(meter);
    }

    public override void FromDocumentUnit(double quantity)
    {
        FromNeutralUnit(GhDecimalLengthInfo.ConvertFromRhinoUnit(quantity, RhinoDocServer.ModelUnitSystem));
    }

    public override void FromNeutralUnit(double neutralAmount)
    {
        FromFeetAmount(neutralAmount / 0.3048);
    }

    public override bool SupportDocumentUnitConversion => true;

    public override double ToDocumentUnit()
    {
        return GhDecimalLengthInfo.ConvertToRhinoUnit(ToNeutralUnit(), RhinoDocServer.ModelUnitSystem);
    }

    public override double ToNeutralUnit()
    {
        return RawValue * 0.3048;
    }

    public string ToString(bool removeDash, bool removeSpace, bool ignoreFtZero, bool ignoreInchZero)
    {
        var negative = Negative ? "-" : "";

        var space = removeSpace ? "" : " ";
        var dashSpace = removeDash ? "" : "-" + space;

        var result = "";

        if (!ignoreFtZero && FeetIntegerPart == 0 || FeetIntegerPart != 0)
            result += $"{FeetIntegerPart}'";

        var noInchPart = true;
        if (FeetIntegerPart != 0 || !(ignoreInchZero || ignoreFtZero) && InchIntegerPart == 0 || InchIntegerPart != 0)
        {
            noInchPart = false;
            result += $"{space}{dashSpace}{InchIntegerPart}\"";
        }

        if (InchFractionPartFirst != 0)
        {
            if (result.EndsWith("\""))
                result = result.Substring(0, result.Length - 1);

            if (!noInchPart || result.Length != 0 && !removeSpace)
                result += " ";

            result += $"{InchFractionPartFirst}/{InchFractionPartSecond}\"";
        }

        if (result.Length == 0)
            result = "0\"";

        return negative + result;
    }
    public override string ToString()
    {
        return ToString(false, false, false, false);
    }

    public const string Feet = "'";
    public const string Inch = "\"";

    public static bool TryParse(string strData, [NotNullWhen(true)] out GhLengthFeetInch? quantity)
    {
        var str = ReplaceUnicodeFraction(strData.Trim()).Replace("''", "\"");

        if (str.Any(c => !IsAllowedChar(c)) || !IsCharOccurenceAllowed(str))
        {
            quantity = null;
            return false;
        }

        var negative = false;

        if (str.StartsWith("-"))
        {
            negative = true;
            str = str.Substring(1);
        }

        var blocks = str.Trim().SplitWhile(StringUtility.IsNumeric)
            .Select(b => b.Trim()).Where(b => b != string.Empty).ToList();
        var numericPosition = new List<int>();
        var blockCount = 0;

        for (var i = 0; i < blocks.Count; i++)
        {
            if (StringUtility.IsNumeric(blocks[i]))
            {
                ++blockCount;
                numericPosition.Add(i);
            }
        }

        quantity = blockCount switch
        {
            1 => TryParseBlock1(blocks, numericPosition),
            2 => TryParseBlock2(blocks, numericPosition),
            3 => TryParseBlock3(blocks, numericPosition),
            4 => TryParseBlock4(blocks, numericPosition),
            _ => null,
        };

        if (quantity is not null)
            quantity.Negative = negative;

        return quantity is not null;
    }

    private static bool IsAllowedChar(char c)
    {
        return StringUtility.IsNumeric(c) || Feet.Contains(c)
            || Inch.Contains(c) || c == '/' || c == ' ' || c == '-';
    }

    private static bool IsCharOccurenceAllowed(string str)
    {
        var feetCnt = str.Count(c => c == Feet[0]);
        var inchCnt = str.Count(c => c == Inch[0]);
        var slashCnt = str.Count(c => c == '/');
        var dashCnt = str.Count(c => c == '-');

        if (dashCnt > 1 || slashCnt > 1 || feetCnt > 1 || inchCnt > 1) return false;
        if (feetCnt == 0 && inchCnt == 0) return false;

        return true;
    }

    private static GhLengthFeetInch? TryParseBlock1(List<string> blocks, List<int> position)
    {
        // 1"
        // 2'

        if (!CheckBoundary(blocks, position[0] + 1)) return null;

        var feetSymbol = blocks[position[0] + 1] == Feet;
        var inchSymbol = blocks[position[0] + 1] == Inch;

        if (!feetSymbol && !inchSymbol) return null;
        if (feetSymbol)
        {
            // Feet
            return new GhLengthFeetInch(blocks[position[0]], "0", "0", "1");
        }
        else
        {
            // Inch
            return new GhLengthFeetInch("0", blocks[position[0]], "0", "1");
        }
    }

    private static GhLengthFeetInch? TryParseBlock2(List<string> blocks, List<int> position)
    {
        // 1' 1"
        // 1' - 1"
        // 1/2"

        if (!CheckBoundary(blocks, position[1] + 1)) return null;
        if (blocks[position[1] + 1] != Inch) return null;

        if (blocks[position[0] + 1] == "/")
        {
            if (blocks[position[1]] == "0") return null;
            return new GhLengthFeetInch("0", "0", blocks[position[0]], blocks[position[1]]);
        }
        else
        {
            if (!JudgeFeetSymbol(blocks[position[0] + 1])) return null;
            return new GhLengthFeetInch(blocks[position[0]], blocks[position[1]], "0", "1");
        }
    }

    private static GhLengthFeetInch? TryParseBlock3(List<string> blocks, List<int> position)
    {
        // 1 2/3"

        if (!CheckBoundary(blocks, position[2] + 1)) return null;
        if (position[1] != position[0] + 1) return null;
        if (blocks[position[2] + 1] != Inch || blocks[position[1] + 1] != "/") return null;
        if (blocks[position[2]] == "0") return null;

        return new GhLengthFeetInch("0", blocks[position[0]],
            blocks[position[1]], blocks[position[2]]);
    }

    private static GhLengthFeetInch? TryParseBlock4(List<string> blocks, List<int> position)
    {
        // 1' 2 3/4"
        // 1' - 2 3/4"

        if (!CheckBoundary(blocks, position[3] + 1)) return null;
        if (!JudgeFeetSymbol(blocks[position[0] + 1])
            || blocks[position[2] + 1] != "/"
            || blocks[position[3] + 1] != Inch
            || position[2] != position[1] + 1) return null;

        return new GhLengthFeetInch(blocks[position[0]], blocks[position[1]],
            blocks[position[2]], blocks[position[3]]);
    }

    private static bool JudgeFeetSymbol(string strData)
    {
        var str = strData.Replace(" ", "");
        return str == Feet || str == Feet + "-";
    }

    private static bool CheckBoundary<T>(IList<T> array, int index)
    {
        return index >= 0 && index < array.Count;
    }

    private static string ReplaceUnicodeFraction(string str)
    {
        return str.Replace("¼", " 1/4")
            .Replace("½", " 1/2")
            .Replace("¾", " 3/4")
            .Replace("⅐", " 1/7")
            .Replace("⅑", " 1/9")
            .Replace("⅒", " 1/10")
            .Replace("⅓", " 1/3")
            .Replace("⅔", " 2/3")
            .Replace("⅕", " 1/5")
            .Replace("⅖", " 2/5")
            .Replace("⅗", " 3/5")
            .Replace("⅘", " 4/5")
            .Replace("⅙", " 1/6")
            .Replace("⅚", " 5/6")
            .Replace("⅛", " 1/8")
            .Replace("⅜", " 3/8")
            .Replace("⅝", " 5/8")
            .Replace("⅞", " 7/8");
    }

    public override GhQuantity Duplicate()
    {
        if (Precise)
        {
            var obj2 = new GhLengthFeetInch(FeetIntegerPart, InchIntegerPart,
            InchFractionPartFirst, InchFractionPartSecond, Precision)
            {
                Precise = true
            };
            obj2.UpdateRawValue();
            return obj2;
        }
        else
        {
            var obj2 = new GhLengthFeetInch();
            obj2.FromFeetAmount(RawValue);
            return obj2;
        }
    }

    public override GhQuantity DuplicateAndNegate()
    {
        if (Precise)
        {
            var obj2 = new GhLengthFeetInch(FeetIntegerPart, InchIntegerPart,
            InchFractionPartFirst, InchFractionPartSecond, Precision)
            {
                Negative = !Negative,
                Precise = true
            };
            obj2.UpdateRawValue();
            return obj2;
        }
        else
        {
            var obj2 = new GhLengthFeetInch();
            obj2.FromFeetAmount(-RawValue);
            return obj2;
        }
    }

    public override int GetHashCode()
    {
        return RawValue.GetHashCode();
    }
    public override bool IsNegative => Negative;
    public (GhLengthFeetInch Whole, GhLengthFeetInch Remainder) GetWholeAndRemainderPart()
    {
        var wholePart = new GhLengthFeetInch(FeetIntegerPart, InchIntegerPart, precision: Precision);
        var remainder = new GhLengthFeetInch(0, 0, InchFractionPartFirst, InchFractionPartSecond, precision: Precision);
        if (IsNegative)
            remainder = (GhLengthFeetInch)(-remainder);

        return (wholePart, remainder);
    }
}
