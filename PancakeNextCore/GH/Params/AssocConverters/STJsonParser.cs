using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PancakeNextCore.GH.Params;
using PancakeNextCore.GH.Params.AssocConverters;



#if NET
using System.Text.Json;
using System.Text.Json.Nodes;
#endif

namespace PancakeNextCore.DataType.AssocConverters;
#if NET
internal sealed class STJsonParser : IJsonParser
{
    public static readonly STJsonParser Instance = new();
    public bool TryParseJson(string json, out GhAssocBase? assoc) => TryParseJsonLight(json, out assoc);
    public static bool TryParseJsonLight(string json, out GhAssocBase? assoc)
    {
        try
        {
            var node = JsonNode.Parse(json);
            assoc = (GhAssocBase?)CreateObjectFromExternalObject(node);
            return true;
        }
        catch (JsonException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidCastException)
        {
        }

        assoc = null;
        return false;
    }

    static object? CreateObjectFromExternalObject(JsonNode? obj)
    {
        return obj switch
        {
            null => null,
            JsonArray jlist => CreateListFromArray(jlist),
            JsonObject jobj => CreateNamedAssocFromObject(jobj),
            JsonValue jsonValue => CreateValueFromJValue(jsonValue),
            _ => throw new NotSupportedException(nameof(obj)),
        };
    }

    static GhAtomList CreateListFromArray(JsonArray array)
    {
        var list = new GhAtomList();
        foreach (var token in array)
        {
            list.Add(CreateObjectFromExternalObject(token));
        }
        return list;
    }

    static GhAssoc CreateNamedAssocFromObject(JsonObject obj)
    {
        var list = new GhAssoc();
        foreach (var token in obj)
        {
            list.Add(token.Key, CreateObjectFromExternalObject(token.Value));
        }
        return list;
    }

    static object? CreateValueFromJValue(JsonValue jv)
    {
        return jv.GetValue<object>();
    }
}
#endif