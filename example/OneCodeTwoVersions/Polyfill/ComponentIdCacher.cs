using GrasshopperIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
internal static class ComponentIdCacher
{
    static readonly Dictionary<Type, Guid> _componentGuids = [];
    public static Guid GetId(Type type)
    {
        if (_componentGuids.TryGetValue(type, out var id)) return id;

        if (type.GetCustomAttribute<IoIdAttribute>()?.Id is not { } idFromAttribute)
        {
            throw new InvalidOperationException($"{type} doesn't have an IoId attribute.");
        }

        return _componentGuids[type] = idFromAttribute;
    }
}
