using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.GH.Params;
using PancakeNextCore.GH.Params.AssocConverters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Components.Association;

[IoId("{C92DF7F3-F383-483B-BFE7-3342788DF589}")]
public sealed class pcJsonToAssoc : PancakeComponent
{
    public pcJsonToAssoc() : base(typeof(pcJsonToAssoc)) { }
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
        set
        {
            _parser = value;
            SetValue(ConfigParserName, _parser ?? "");
        }
    }

    protected override void ReadConfig()
    {
        _parser = GetValue(ConfigParserName, null);
    }
    static readonly string[] ValidParsers = JsonParserLibrary.Parsers.Select(p => p.Name).ToArray();
    protected override InputOption[][] SimpleOptions => [[
            new PickOneOption<string>("Parser", "Pick a parser for parsing JSON. They vary in requirements and performances.", "parser", 
                initialValue: Parser,
                valueNames: ValidParsers,
                validValues: ValidParsers,
                setter: v => Parser = v
                )
            ]];
}
