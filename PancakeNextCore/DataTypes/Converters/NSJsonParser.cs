using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PancakeNextCore.DataTypes.Converters;
internal sealed class NSJsonParser : IJsonParser
{
    public static readonly NSJsonParser Instance = new();
    public bool TryParseJson(string json, out Association? assoc) => TryParseJsonLight(json, out assoc);
    public static bool TryParseJsonLight(string json, out Association? assoc)
    {
        try
        {
            var jobject = JToken.Parse(json);
            assoc = (Association?)CreateObjectFromExternalObject(jobject);
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

    static AtomList CreateListFromArray(JArray array)
    {
        var list = new AtomList();
        foreach (var token in array)
        {
            list.Add(CreateObjectFromExternalObject(token));
        }
        return list;
    }

    static NamedAssociation CreateNamedAssocFromObject(JObject obj)
    {
        var list = new NamedAssociation();
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
