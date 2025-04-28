using GrasshopperIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
internal abstract class ComponentAttributeCacher<TAttribute, TValue> where TAttribute : Attribute
{
    static readonly Dictionary<Type, TValue> _componentAttrs = [];
    public TValue this[Type type]
    {
        get
        {
            if (_componentAttrs.TryGetValue(type, out var value)) return value;

            var attr = type.GetCustomAttribute<TAttribute>();
            if (attr is null && ThrowOnMissing)
                throw new InvalidOperationException($"{type} doesn't have {typeof(TAttribute).Name} attribute.");

            return _componentAttrs[type] = Convert(attr);
        }
    }
    protected virtual bool ThrowOnMissing => true;
    protected abstract TValue Convert(TAttribute? attribute);
}
internal sealed class ComponentIdCacher : ComponentAttributeCacher<IoIdAttribute, Guid>
{
    public static readonly ComponentIdCacher Instance = new();
    protected override Guid Convert(IoIdAttribute? attribute) => attribute!.Id;
}
internal sealed class ComponentSlotCacher : ComponentAttributeCacher<RibbonPositionAttribute, RibbonPositionAttribute>
{
    public static readonly ComponentSlotCacher Instance = new();
    protected override RibbonPositionAttribute Convert(RibbonPositionAttribute? attribute)
        => attribute ?? RibbonPositionAttribute.Default;
    protected override bool ThrowOnMissing => false;
}
internal sealed class ObsoleteCacher : ComponentAttributeCacher<ObsoleteAttribute, bool>
{
    public static readonly ObsoleteCacher Instance = new();
    protected override bool Convert(ObsoleteAttribute? attribute) => attribute is not null;
    protected override bool ThrowOnMissing => false;
}
