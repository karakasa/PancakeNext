using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.GH.Params.AssocConverters;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
[IoId("badd27c5-4a24-48bc-802d-37992a027531")]
public sealed class pcCsvToAssoc : PancakeComponent<pcCsvToAssoc>, IPancakeLocalizable<pcCsvToAssoc>
{
    public pcCsvToAssoc() { }
    public pcCsvToAssoc(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.ConstructAssociativeArrayFromCSV;
    public static string StaticLocalizedDescription => Strings.CreateAListOfAssociativeArraysFromCSVLinesSeeExampleForMoreInformation;
    protected override void RegisterInputs()
    {
        AddParam<TextParameter>("csv", Access.Twig);
        AddParam("delimiter", ",");
    }

    protected override void RegisterOutputs()
    {
        AddParam<AssociationParameter>("assoc", Access.Twig);
    }

    private bool headerless = false;

    protected override void Process(IDataAccess access)
    {
        access.GetTwig<string>(0, out var lineTwig);
        access.GetItem(1, out string delimiter);

        var lines = lineTwig.Items.ToList();

        if (lines.Count == 1)
        {
            lines[0] = FileIo.ReadContentIfIsFile(lines[0]);

            var splittedLines = lines[0].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            lines.Clear();
            lines.AddRange(splittedLines);
        }

        lines.RemoveAll(l => l.Trim().Length == 0);

        if (string.IsNullOrEmpty(delimiter))
        {
            access.AddError("Wrong delimiter", Strings.DelimiterIsEmpty);
            return;
        }

        var delimiterArray = new[] { delimiter };

        var assocs = new List<GhAssoc>
        {
            Capacity = lines.Count
        };

        string[]? headers = null;

        if (!headerless)
        {
            headers = GetHeaders(lines.First(), delimiterArray);
        }

        foreach (var it in lines.Skip(headerless ? 0 : 1))
        {
            var lineContent = it.Split(delimiterArray, StringSplitOptions.None);
            assocs.Add(ConvertLineToAssoc(headers, lineContent));
        }

        access.SetTwig(0, Garden.TwigFromList(assocs));
    }

    private string[] GetHeaders(string headerLine, string[] delimiterArray)
    {
        if (_recognizeFormat)
            return headerLine.Split(delimiterArray, StringSplitOptions.None)
                .Select(UnquoteString).ToArray();
        else
            return headerLine.Split(delimiterArray, StringSplitOptions.None);
    }

    private static string UnquoteString(string strData)
    {
        var str = strData.Trim();
        if (str.StartsWith("\"") && str.EndsWith("\""))
        {
            str = str.Substring(1, str.Length - 2);
        }
        return str;
    }

    private static string? GetHeaderName(string[]? headers, int index)
    {
        if (headers == null || index >= headers.Length || index < 0)
            return null;

        if (string.IsNullOrEmpty(headers[index]))
            return null;

        return headers[index];
    }

    private GhAssoc ConvertLineToAssoc(string[]? headers, string[] content)
    {
        var assoc = new GhAssoc(content.Length);

        for (var i = 0; i < content.Length; i++)
        {
            var header = GetHeaderName(headers, i);
            var result = _recognizeFormat ? AssocStringUtility.RecognizeType(content[i]) : content[i];
            if (header == null)
            {
                assoc.Add(result);
            }
            else
            {
                assoc.Add(header, result);
            }
        }

        return assoc;
    }

    private bool _recognizeFormat = true;
    private bool _parseAsQuantity = false;

    public bool RecognizeFormat
    {
        get => _recognizeFormat;
        set => SetValue(RecognizeTypeOption, _recognizeFormat = value);
    }

    public bool RecognizeQuantity
    {
        get => _parseAsQuantity;
        set => SetValue(ParseAsQuantityOption, _parseAsQuantity = value);
    }

    protected override void ReadConfig()
    {
        _recognizeFormat = GetValue(RecognizeTypeOption, true);
        _parseAsQuantity = GetValue(ParseAsQuantityOption, false);
    }

    protected override InputOption[][] SimpleOptions => [[
            new ToggleOption("Recognize type", "Converts text into corresponding types if possible, e.g. into numbers.", RecognizeFormat, x => RecognizeFormat = x, "Recognize type", "Keep text"),
            new ToggleOption("Recognize quantity", "Converts text into quantities if possible.", RecognizeQuantity, x => RecognizeQuantity = x, "Recognize quantity", "Keep text")
            ]];

    private const string RecognizeTypeOption = "RecognizeType";
    private const string ParseAsQuantityOption = "ParseAsQuantity";
}