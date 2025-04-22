using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params.AssocConverters;
internal static class JsonParserLibrary
{
    public static List<(string Name, IJsonParser Instance)> Parsers = new()
    {
        ("Pancake", BuiltinJsonParser.Instance),
        ("NSJ", NSJsonParser.Instance),
#if NET
        ("STJ", STJsonParser.Instance)
#endif
    };

    public static IJsonParser DefaultParser { get; set; } = BuiltinJsonParser.Instance;

    public static IJsonParser GetParser(string? name)
    {
        if (string.IsNullOrEmpty(name)) return DefaultParser;
        foreach (var it in Parsers)
            if (it.Item1 == name)
                return it.Item2;
        return DefaultParser;
    }
}
