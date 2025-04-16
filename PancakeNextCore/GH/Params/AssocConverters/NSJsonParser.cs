using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PancakeNextCore.GH.Params.AssocConverters;
internal sealed class NSJsonParser : IJsonParser
{
    public static readonly NSJsonParser Instance = new();
    public bool TryParseJson(string json, out GhAssocBase? assoc) => TryParseJsonLight(json, out assoc);
    public static bool TryParseJsonLight(string json, out GhAssocBase? assoc)
    {
        try
        {
            var jobject = JToken.Parse(json);
            assoc = (GhAssocBase?)CreateObjectFromExternalObject(jobject);
            return true;
        }
        catch (JsonReaderException)
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

    static object? CreateObjectFromExternalObject(JToken? obj)
    {
        return obj switch
        {
            null => null,
            JArray jlist => CreateListFromArray(jlist),
            JObject jobj => CreateNamedAssocFromObject(jobj),
            JValue jv => CreateValueFromJValue(jv),
            _ => throw new NotSupportedException(nameof(obj)),
        };
    }

    static GhAtomList CreateListFromArray(JArray array)
    {
        var list = new GhAtomList();
        foreach (var token in array)
        {
            list.Add(CreateObjectFromExternalObject(token));
        }
        return list;
    }

    static GhAssoc CreateNamedAssocFromObject(JObject obj)
    {
        var list = new GhAssoc();
        foreach (var token in obj)
        {
            list.Add(token.Key, CreateObjectFromExternalObject(token.Value));
        }
        return list;
    }

    static object? CreateValueFromJValue(JValue jv)
    {
        return jv.Value;
    }
}
