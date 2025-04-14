using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PancakeNextCore.DataType.AssocConverters;
using PancakeNextCore.DataType;

#if NET
using System.Text.Json;
using System.Text.Json.Nodes;
#endif

namespace PancakeNextCore.DataType.AssocConverters;
#if NET
internal sealed class STJsonParser : IJsonParser
{
    public static readonly STJsonParser Instance = new();
    public bool TryParseJson(string json, out Association? assoc) => TryParseJsonLight(json, out assoc);
    public static bool TryParseJsonLight(string json, out Association? assoc)
    {
        try
        {
            var node = JsonNode.Parse(json);
            assoc = (Association?)CreateObjectFromExternalObject(node);
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

    static AtomList CreateListFromArray(JsonArray array)
    {
        var list = new AtomList();
        foreach (var token in array)
        {
            list.Add(CreateObjectFromExternalObject(token));
        }
        return list;
    }

    static NamedAssociation CreateNamedAssocFromObject(JsonObject obj)
    {
        var list = new NamedAssociation();
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