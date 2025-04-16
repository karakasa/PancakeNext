using PancakeNextCore.DataType.AssocConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params.AssocConverters;
internal static class JsonParserLibrary
{
    public static Dictionary<string, IJsonParser> Parsers = new()
    {
        ["Pancake"] = BuiltinJsonParser.Instance,
        ["Newtonsoft"] = NSJsonParser.Instance,
#if NET
        [".NET"] = STJsonParser.Instance
#endif
    };

    public static IJsonParser DefaultParser { get; set; } = BuiltinJsonParser.Instance;

    public static IJsonParser GetParser(string? name)
    {
        if (name is null) return DefaultParser;
        return Parsers.TryGetValue(name, out var parser) ? parser : DefaultParser;
    }
}
