using System;
using System.Collections.Generic;
using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Dataset;
using PancakeNextCore.GH.Params;
using PancakeNextCore.GH.Params.AssocConverters;
using PancakeNextCore.Interfaces;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("assoc", 2)]
[IoId("89e37b0a-e1c3-43e9-b4cb-1c33b6d20627")]
public sealed class pcAssocToString : PancakeComponent<pcAssocToString>, IPancakeLocalizable<pcAssocToString>
{
    public pcAssocToString() { }
    public pcAssocToString(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.AssociativeArrayToString;
    public static string StaticLocalizedDescription => Strings.ConvertAnAssociativeArrayToAStringOfCertainStyleSuchAsJsonPythonEtcRightClickTheComponentToChooseStyle;
    protected override void RegisterInputs()
    {
        AddParam<QuantityParameter>("assoc");
    }
    protected override void RegisterOutputs()
    {
        AddParam<TextParameter>("string");
    }

    private string? HandleNull()
    {
        return ConversionType switch
        {
            StringConversionType.Json or StringConversionType.AllStringJson => "null",
            StringConversionType.Association => "Null",
            StringConversionType.Python => "None",
            _ => null,
        };
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out GhAssocBase obj);

        string? resultString = null;

        if (obj is null)
        {
            resultString = HandleNull();
            if (resultString is null)
            {
                access.AddError("Null value", "Input is null.");
                return;
            }
        }
        else
        {
            resultString = AssociationStringifier.ToString(obj, ConversionType);
        }

        if (resultString is null)
        {
            access.AddError("Wrong input", "Unknown input type.");
            return;
        }

        access.SetItem(0, resultString);
    }

    private static readonly Pick<StringConversionType>[] Styles =
        [
        new("Json", StringConversionType.Json, "Json", "Json format, like {\"a\": 123} ."),
        new("Url", StringConversionType.Querystring, "Url", "Querystring format, like ?a=123 .\r\nNested objects are disallowed."),
        new("Python", StringConversionType.Python, "Python dict", "Python dict format, like {'a': 123} ."),
        new("Assoc", StringConversionType.Association, "Association", "Association format, like <| \"a\" -> 123 |> ."),
        ];

    private StringConversionType _conversionType;
    protected override void ReadConfig()
    {
        _conversionType = GetValue(StyleOptionName, "") switch
        {
            "Querystring" => StringConversionType.Querystring,
            "Python" => StringConversionType.Python,
            "Association" => StringConversionType.Association,
            _ => StringConversionType.Json
        };

        UpdateMessage();
    }

    private void UpdateMessage()
    {
        Label = ConversionType switch
        {
            StringConversionType.Json => "Json",
            StringConversionType.AllStringJson => "Json*",
            StringConversionType.Querystring => "Url",
            StringConversionType.Association => "Association",
            StringConversionType.Python => "Python",
            _ => "?"
        };
    }

    public StringConversionType ConversionType
    {
        get => _conversionType;
        set
        {
            SetValue(StyleOptionName, (_conversionType = value) switch
            {
                StringConversionType.Querystring => "Querystring",
                StringConversionType.Python => "Python",
                StringConversionType.Association => "Association",
                _ => "Json"
            });

            UpdateMessage();
        }
    }

    private const string StyleOptionName = "Style";

    protected override InputOption[][] SimpleOptions => [[
            new PickOneOption<StringConversionType>("Style", ConversionType, x => ConversionType = x, Styles)
            ]];
}