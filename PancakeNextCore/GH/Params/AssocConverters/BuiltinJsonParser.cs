using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PancakeNextCore.GH.Params.AssocConverters;

internal sealed class BuiltinJsonParser : IJsonParser
{
    [Conditional("DEBUG")]
    private static void StopHere()
    {
        ;
    }

    public static readonly BuiltinJsonParser Instance = new();
    public bool TryParseJson(string json, out GhAssocBase? assoc) => TryParseJsonLight(json, out assoc);
    public static bool TryParseJsonLight(string json, out GhAssocBase? assoc)
    {
        var str = json.Trim();

        try
        {
            if (str.Length < 2)
            {
                assoc = null;
                return false;
            }

            var firstChar = str[0];
            var lastChar = str[str.Length - 1];

            if ((firstChar != '{' || lastChar != '}') && (firstChar != '[' || lastChar != ']'))
            {
                assoc = null;
                return false;
            }

            if (firstChar == '{' && TryParseJsonIndex(ref str, 0, out var subAssoc, out var processed)
                && processed == str.Length)
            {
                assoc = subAssoc;
                return true;
            }

            if (TryParseJsonList(ref str, 0, out var subList, out processed)
                && processed == str.Length)
            {
                assoc = subList;
                return true;
            }

            StopHere();
            subAssoc = null;
            assoc = null;
            return false;
        }
        catch (Exception ex)
        {
            StopHere();
        }

        StopHere();
        assoc = null;
        return false;
    }

    private static bool TryParseJsonIndex(ref string json, int startIndex,
        [NotNullWhen(true)] out GhAssoc? assoc, out int processedChars)
    {
        int whitespace;

        processedChars = SkipWhitespace(ref json, startIndex);
        var newStart = processedChars + startIndex;
        if (json[newStart] != '{')
        {
            StopHere();
            assoc = null;
            processedChars = 0;
            return false;
        }

        assoc = new GhAssoc();
        var index = newStart;

        ++index;
        ++processedChars;

        whitespace = SkipWhitespace(ref json, index);
        index += whitespace;
        processedChars += whitespace;

        if (json[index] == '}')
        {
            ++processedChars;
            return true;
        }

        for (; index < json.Length;)
        {
            if (!TryParseJsonString(ref json, index, out var name, out var processed))
            {
                StopHere();
                assoc?.Clear();
                assoc = null;
                processedChars = 0;
                return false;
            }

            index += processed;
            processedChars += processed;

            whitespace = SkipWhitespace(ref json, index);
            index += whitespace;
            processedChars += whitespace;

            if (json[index] != ':')
            {
                StopHere();
                assoc?.Clear();
                assoc = null;
                processedChars = 0;
                return false;
            }

            index++;
            processedChars++;

            if (!TryParseJsonValue(ref json, index, out var value, out processed))
            {
                StopHere();
                assoc?.Clear();
                assoc = null;
                processedChars = 0;
                return false;
            }

            assoc.Add(name, value);

            index += processed;
            processedChars += processed;

            whitespace = SkipWhitespace(ref json, index);
            index += whitespace;
            processedChars += whitespace;

            if (json[index] == ',')
            {
                processedChars++;
                index++;
                continue;
            }
            else if (json[index] == '}')
            {
                processedChars++;
                return true;
            }
            else
            {
                break;
            }
        }

        StopHere();
        assoc?.Clear();
        assoc = null;
        processedChars = 0;
        return false;
    }

    private static bool TryParseJsonList(ref string json, int startIndex,
        [NotNullWhen(true)] out GhAtomList? assoc, out int processedChars)
    {
        int whitespace;

        processedChars = SkipWhitespace(ref json, startIndex);
        var newStart = processedChars + startIndex;
        if (json[newStart] != '[')
        {
            StopHere();
            assoc = null;
            processedChars = 0;
            return false;
        }

        assoc = new GhAtomList();
        var index = newStart;

        ++index;
        ++processedChars;

        whitespace = SkipWhitespace(ref json, index);
        index += whitespace;
        processedChars += whitespace;

        if (json[index] == ']')
        {
            ++processedChars;
            return true;
        }

        for (; index < json.Length;)
        {
            if (!TryParseJsonValue(ref json, index, out var value, out var processed))
            {
                StopHere();
                assoc?.Clear();
                assoc = null;
                processedChars = 0;
                return false;
            }

            assoc.Add(value);

            index += processed;
            processedChars += processed;

            whitespace = SkipWhitespace(ref json, index);
            index += whitespace;
            processedChars += whitespace;

            if (json[index] == ',')
            {
                processedChars++;
                index++;
                continue;
            }
            else if (json[index] == ']')
            {
                processedChars++;
                return true;
            }
            else
            {
                break;
            }
        }

        StopHere();
        assoc?.Clear();
        assoc = null;
        processedChars = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsWhiteSpace(char c) => c == ' ' || c == '\r' || c == '\n' || c == '\t';

    private static int SkipWhitespace(ref string json, int startIndex)
    {
        var processed = 0;

        for (var index = startIndex; index < json.Length; index++, processed++)
        {
            if (!IsWhiteSpace(json[index]))
                break;
        }

        return processed;
    }

    private static bool TryParseJsonString(ref string json, int startIndex,
         [NotNullWhen(true)] out string? str, out int processedChars)
    {
        processedChars = SkipWhitespace(ref json, startIndex);
        var newStart = processedChars + startIndex;

        if (json[newStart] != '"')
        {
            StopHere();
            str = null;
            processedChars = 0;
            return false;
        }

        var curStr = "";
        for (var index = newStart + 1; ; index++, processedChars++)
        {
            if (index >= json.Length)
            {
                StopHere();
                str = null;
                processedChars = 0;
                return false;
            }

            var ch = json[index];
            if (ch == '\\')
            {
                if (index + 1 >= json.Length)
                {
                    StopHere();
                    str = null;
                    processedChars = 0;
                    return false;
                }

                ++index;
                ++processedChars;

                ch = json[index];

                if (ch == 'u')
                {
                    ++index;
                    ++processedChars;

                    var hex = json.Substring(index, 4);

                    curStr += (char)int.Parse(hex, NumberStyles.HexNumber);

                    index += 3;
                    processedChars += 3;
                }
                else
                {
                    var unescaped = AssocStringUtility.UnescapeJsonSpecialChar(ch);
                    if (unescaped == char.MinValue)
                    {
                        StopHere();
                        str = null;
                        processedChars = 0;
                        return false;
                    }

                    curStr += unescaped;
                }
            }
            else if (ch == '"')
            {
                str = curStr;
                processedChars += 2;
                return true;
            }
            else if (ch == '\r' || ch == '\n' || ch == '\t' || ch == '\0')
            {
                StopHere();
                str = null;
                processedChars = 0;
                return false;
            }
            else
            {
                curStr += ch;
            }
        }
    }

    private static bool TryParseJsonBool(ref string json, int startIndex,
        out bool boolean, out int processedChars)
    {
        processedChars = SkipWhitespace(ref json, startIndex);
        var newStart = processedChars + startIndex;

        if (json.Length >= newStart + 4 && json.Substring(newStart, 4) == "true")
        {
            boolean = true;
            processedChars += 4;
            return true;
        }

        if (json.Length >= newStart + 5 && json.Substring(newStart, 5) == "false")
        {
            boolean = false;
            processedChars += 5;
            return true;
        }

        StopHere();
        boolean = default;
        processedChars = 0;
        return false;
    }

    private static bool TryParseJsonNull(ref string json, int startIndex, out int processedChars)
    {
        processedChars = SkipWhitespace(ref json, startIndex);
        var newStart = processedChars + startIndex;

        if (json.Length >= newStart + 4 && json.Substring(newStart, 4) == "null")
        {
            processedChars += 4;
            return true;
        }

        StopHere();
        processedChars = 0;
        return false;
    }

    private static bool TryParseJsonNumber(ref string json, int startIndex,
         [NotNullWhen(true)] out object? number, out int processedChars)
    {
        processedChars = SkipWhitespace(ref json, startIndex);
        var newStart = processedChars + startIndex;

        var curStr = "";

        for (var i = newStart; i < json.Length; i++)
        {
            if (!IsNumeric(json[i]))
            {
                number = ParseNumberString(curStr);
                processedChars += curStr.Length;
                return true;
            }

            curStr += json[i];
        }

        StopHere();
        number = null;
        processedChars = 0;
        return false;
    }

    private static object ParseNumberString(string str)
    {
        var lastChar = str[str.Length - 1];

        if (str[0] == '+' || lastChar == 'e' || lastChar == 'E' || lastChar == '.')
        {
            StopHere();
            throw new FormatException();
        }

        if (str.Length >= 2 && str[0] == '0' && str[1] >= '0' && str[1] <= '9')
        {
            StopHere();
            throw new FormatException();
        }

        if (str.Length >= 3 && str[0] == '-' && str[1] == '0' && str[2] >= '0' && str[2] <= '9')
        {
            StopHere();
            throw new FormatException();
        }

        if (str[0] == '.')
        {
            StopHere();
            throw new FormatException();
        }

        if (str.Length >= 2 && str[0] == '-' && str[1] == '.')
        {
            StopHere();
            throw new FormatException();
        }

        var isDouble = false;

        for (var i = 0; i < str.Length; i++)
        {
            if (str[i] == '.')
                if (isDouble)
                {
                    StopHere();
                    throw new FormatException();
                }
                else
                {
                    isDouble = true;
                }

            if (str[i] == 'e' || str[i] == 'E')
            {
                if (i != 0)
                {
                    if (str[i - 1] == '.')
                    {
                        StopHere();
                        throw new FormatException();
                    }
                }

                isDouble = true;
            }
        }

        try
        {
            if (isDouble)
            {
                return Convert.ToDouble(str, CultureInfo.InvariantCulture);
            }
            else
            {
                return Convert.ToInt32(str, CultureInfo.InvariantCulture);
            }
        }
        catch (OverflowException)
        {
            return str;
        }
    }

    private static bool TryParseJsonValue(ref string json, int startIndex,
        out object? value, out int processedChars)
    {
        processedChars = SkipWhitespace(ref json, startIndex);
        var newStart = processedChars + startIndex;

        var processed = 0;

        switch (json[newStart])
        {
            case '"':
                if (!TryParseJsonString(ref json, newStart, out var str, out processed))
                {
                    StopHere();
                    value = null;
                    processedChars = 0;
                    return false;
                }

                value = str;
                processedChars += processed;
                return true;
            case '{':
                if (!TryParseJsonIndex(ref json, newStart, out var assoc, out processed))
                {
                    StopHere();
                    value = null;
                    processedChars = 0;
                    return false;
                }

                value = assoc;
                processedChars += processed;
                return true;
            case '[':
                if (!TryParseJsonList(ref json, newStart, out var list, out processed))
                {
                    StopHere();
                    value = null;
                    processedChars = 0;
                    return false;
                }

                value = list;
                processedChars += processed;
                return true;
            case 't':
            case 'f':
                if (!TryParseJsonBool(ref json, newStart, out var boolean, out processed))
                {
                    StopHere();
                    value = null;
                    processedChars = 0;
                    return false;
                }

                value = boolean;
                processedChars += processed;
                return true;
            case 'n':
                if (!TryParseJsonNull(ref json, newStart, out processed))
                {
                    StopHere();
                    value = null;
                    processedChars = 0;
                    return false;
                }

                value = null;
                processedChars += processed;
                return true;
            default:
                if (IsNumeric(json[newStart]))
                {
                    if (!TryParseJsonNumber(ref json, newStart, out var num, out processed))
                    {
                        StopHere();
                        value = null;
                        processedChars = 0;
                        return false;
                    }

                    value = num;
                    processedChars += processed;
                    return true;
                }
                break;
        }

        StopHere();
        value = null;
        processedChars = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumeric(char c) => c >= '0' && c <= '9' || c == '.' || c == '-'
        || c == '+' || c == 'E' || c == 'e';
}