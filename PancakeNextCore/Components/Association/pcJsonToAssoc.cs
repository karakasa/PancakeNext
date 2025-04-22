using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.GH.Params.AssocConverters;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Components.Association;

[IoId("{C92DF7F3-F383-483B-BFE7-3342788DF589}")]
[ComponentCategory("assoc", 1)]
public sealed class pcJsonToAssoc : PancakeComponent<pcJsonToAssoc>, IPancakeLocalizable<pcJsonToAssoc>
{
    public pcJsonToAssoc() { }
    public pcJsonToAssoc(IReader reader) : base(reader) { }

    protected override void RegisterInputs()
    {
        AddParam<TextParameter>("json");
    }

    protected override void RegisterOutputs()
    {
        AddParam<AssociationParameter>("assoc");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out string str);

        if (str.StartsWith("<"))
        {
            if (str.StartsWith("<?xml") || str.StartsWith("<? xml"))
            {
                access.AddError("Parse failure", "Input is XML content. Use 'XML to Assoc' instead.");
                return;
            }
        }

        if (JsonParserLibrary.GetParser(Parser).TryParseJson(str, out var assoc))
        {
            access.SetItem(0, assoc);
        }
        else
        {
            access.AddError("Parse failure", "Fail to parse. Be advised that Pancake may reject non-standard json formats.");
        }
    }

    private const string ConfigParserName = "Parser";
    private string? _parser = "";
    public string? Parser
    {
        get => _parser;
        set => SetValue(ConfigParserName, _parser ??= value ?? "");
    }

    protected override void ReadConfig()
    {
        _parser = GetValue(ConfigParserName, null);
    }
    static readonly Pick<string>[] ValidParsers = CreateParsers().ToArray();

    private static IEnumerable<Pick<string>> CreateParsers()
    {
        foreach (var it in JsonParserLibrary.Parsers)
        {
            (string, string, string) nameDesc = it.Name switch
            {
                "Pancake" => ("Pancake", "Pancake Built-in Parser", "Use the built-in JSON parser, identical to GH1 behavior."),
                "NSJ" => ("NS", "Newtonsoft.Json", "Use Newtonsoft.Json. This is the most unrestrictive option.\r\nThe parser would allow most formats."),
                "STJ" => ("STJ", "System.Text.Json", "Use System.Text.Json from the .NET runtime."),
                _ => (it.Name, it.Name, it.Name)
            };

            yield return new Pick<string>(nameDesc.Item1, it.Name, nameDesc.Item2, nameDesc.Item3);
        }
    }

    protected override InputOption[][] SimpleOptions => [[
            new PickOneOption<string>("Parser", Parser, v => Parser = v, ValidParsers)
            ]];

    public static string StaticLocalizedName => "Json to Assoc";

    public static string StaticLocalizedDescription => "Converts a json string to Assoc object.\r\nUse 'Assoc to String' to convert assoc to json.";
}
