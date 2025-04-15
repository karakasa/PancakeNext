using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using GrasshopperIO;
using PancakeNextCore.Polyfill;
using PancakeNextCore.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Linq;

namespace PancakeNextCore.DataType;

[IoId("22B79783-C674-4BC4-AFBA-014C94D727BD")]
public sealed class GhAssoc : GhAssocBase
{
    public GhAssoc()
    {
    }
    public GhAssoc(int capacity)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        EnsureData(capacity);
    }

    private static readonly Name IoLabelName = new("Name");

    public GhAssoc(IReader reader) : base(reader)
    {
        var count = Length;
        if (count > 0)
        {
            EnsureData(count);
            Names.AddRange(reader.StringArray(IoLabelName));
        }
    }

    public override void Store(IWriter writer)
    {
        base.Store(writer);

        if (!HasValues) return;

        writer.StringArray(IoLabelName, [.. Names]);
    }

    internal List<string?>? Names { get; private set; }

    internal bool IsValid => Names?.Count == Values?.Count;

    static List<T> EnsureListCapacity<T>(List<T>? list, int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

        if (list is null)
        {
            list = new List<T>(capacity);
        }
        else if (list.Capacity < capacity)
        {
            list.Capacity = capacity;
        }

        return list;
    }

    [MemberNotNull(nameof(Names))]
    protected override void EnsureData(int anticipatedCapacity = 0)
    {
        if (anticipatedCapacity > 0)
        {
            Names = EnsureListCapacity(Names, anticipatedCapacity);
            Values = EnsureListCapacity(Values, anticipatedCapacity);
        }
        else
        {
            Names ??= [];
            Values ??= [];
        }
    }
    public void MergeWith(GhAssoc? another)
    {
        if (another is null)
        {
            throw new NullReferenceException(nameof(another));
        }

        if (!another.IsValid)
        {
            throw new ArgumentException("The other association is invalid.", nameof(another));
        }

        if (!another.HasValues) return;

        EnsureData();

        Names.AddRange(another.Names);
        Values.AddRange(another.Values);
    }

    public void Add(object? item)
    {
        Add(null, item);
    }

    public void Add(string? name, object? item)
    {
        EnsureData();

        Names.Add(name);
        Values.Add(item);
    }

    public bool TryGet(string name, out object? output)
    {
        if (!HasValues)
        {
            output = null;
            return false;
        }

        for (var i = 0; i < Length; i++)
        {
            if (name.Equals(Names[i]))
            {
                output = Values[i];
                return true;
            }
        }

        output = null;
        return false;
    }

    internal bool TryGetContent(string name, out object? output)
    {
        if (!HasValues)
        {
            output = null;
            return false;
        }

        if (TryGet(name, out output))
            return true;

        if (name.StartsWith("@") && !name.TryParseSubstrAsInt(1, out var indice))
        {
            if (!IsOutOfRange(indice))
            {
                output = Values[indice];
                return true;
            }
        }

        var lastIndex = name.LastIndexOf("@");
        if (lastIndex == -1 || !name.TryParseSubstrAsInt(lastIndex + 1, out indice))
        {
            output = null;
            return false;
        }

#if NET
        var realName = name.AsSpan(0, lastIndex);
#else
        var realName = name.Substring(0, lastIndex);
#endif

        var index = 0;

        for (var i = 0; i < Length; i++)
        {
            if (StringUtility.Equals(realName, Names[i]))
            {
                if (index == indice)
                {
                    output = Values[i];
                    return true;
                }

                ++index;
            }
        }

        output = null;
        return false;
    }

    public IEnumerable<string> GetNameWithIndexes()
    {
        if (!HasValues)
            yield break;

        var index = 0;

        foreach (var it in Names)
        {
            yield return it ?? index.ToString();
            ++index;
        }
    }

    public IEnumerable<string> GetJsonNames()
    {
        if (!HasValues)
            yield break;

        var index = 0;

        foreach (var it in Names)
        {
            yield return it ?? "item" + index.ToString();
            ++index;
        }
    }

    public GhAssoc(IEnumerable<object?> list) : this(null, list)
    {
    }

    public GhAssoc(IEnumerable<string?>? names, IEnumerable<object?> list)
    {
        Values = [.. list];

        if (names is null)
        {
            Names = [.. Enumerable.Repeat<string?>(null, Values.Count)];
        }
        else
        {
            Names = [.. names];

            if (!IsValid)
            {
                throw new ArgumentException("The number of names and values must be the same.");
            }
        }
    }

    public GhAssoc Clone()
    {
        var tuple = new GhAssoc();
        tuple.MergeWith(this);
        return tuple;
    }

    internal override GhAssocBase GenericClone() => Clone();

    public override string ToString()
    {
        if (_principleIndex == 0 && HasValues && Values[0] is string str)
            return str;

        return GetDescriptiveString();
    }
    private IEnumerable<string> GetItemNames()
    {
        if (!HasValues) 
            yield break;

        for (var i = 0; i < Length; i++)
        {
            yield return (Names[i] != null ? $"{Names[i]}: " : "") + ObjectToString(Values[i]);
        }
    }

    public string GetDescriptiveString()
    {
        if (!HasValues)
            return "<>";

        var types = string.Join(", ", GetItemNames());
        return $"<{types}>";
    }

    public int FindIndexByName(string name)
    {
        if (!HasValues) return -1;

        for (var i = 0; i < Names.Count; i++)
            if (Names[i] == name)
                return i;

        return -1;
    }

    public bool SetPrincipleValue(string name)
    {
        var index = FindIndexByName(name);

        if (index < 0)
            return false;

        return SetPrincipleValue(index);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, this))
            return true;

        if (obj is not GhAssoc assoc)
            return false;

        return Equals(assoc);
    }

    public override void Clear()
    {
        Names?.Clear();
        Values?.Clear();

        Names = null;
        Values = null;
    }

    public bool Equals(GhAssoc another)
    {
        if (another is null) 
            return false;

        var thisLength = Length;

        if (another.Length != thisLength)
            return false;

        for (var i = 0; i < thisLength; i++)
        {
            if (!string.Equals(Names![i], another.Names![i])) return false;
            if (!Equals(Values![i], another.Values![i])) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        if (!HasValues) return 0;

        return unchecked(-940306134
            + EqualityComparer<List<string?>>.Default.GetHashCode(Names) * 7
            + EqualityComparer<List<object?>>.Default.GetHashCode(Values) * 13);
    }

    public static bool operator ==(GhAssoc assoc1, GhAssoc assoc2)
    {
        return assoc1.Equals(assoc2);
    }

    public static bool operator !=(GhAssoc assoc1, GhAssoc assoc2)
    {
        return !(assoc1 == assoc2);
    }

    /*public bool TryGetNode(string name, out INodeQueryReadCapable node)
    {
        TryGetNode(name, out var node2, false);
        node = node2 as INodeQueryReadCapable;
        return node != null;
    }

    public bool TryGetNode(string name, out INodeQueryWriteCapable node, bool createIfNotExist)
    {
        if (!TryGetExtended(name, out var obj))
        {
            if (createIfNotExist)
            {
                var assoc2 = new Association();
                Add(name, assoc2);
                node = assoc2;
                return true;
            }

            node = null;
            return false;
        }

        if (obj is INodeQueryWriteCapable assoc)
        {
            node = assoc;
            return true;
        }

        node = null;
        return false;
    }

    public bool TryGetContent(string attributeName, out object content)
    {
        return TryGetExtended(attributeName, out content);
    }

    public bool SetContent(string attributeName, object content)
    {
        var index = _names.IndexOf(attributeName);
        if (index == -1)
        {
            Add(attributeName, content);
            return true;
        }

        Values[index] = content;
        return true;
    }

    public IEnumerable<KeyValuePair<string, INodeQueryReadCapable>> GetNodes()
    {
        for (var i = 0; i < _names.Count; i++)
        {
            if (Values[i] is INodeQueryReadCapable inode)
                yield return new KeyValuePair<string, INodeQueryReadCapable>(_names[i], inode);
        }
    }

    public IEnumerable<KeyValuePair<string, object>> GetAttributes()
    {
        for (var i = 0; i < _names.Count; i++)
        {
            if (!(Values[i] is INodeQueryReadCapable) && _names[i] != null)
                yield return new KeyValuePair<string, object>(_names[i], Values[i]);
        }
    }

    public IEnumerable<KeyValuePair<string, object>> GetNamedValues()
    {
        for (var i = 0; i < _names.Count; i++)
        {
            if (_names[i] != null)
                yield return new KeyValuePair<string, object>(_names[i], Values[i]);
        }
    }*/

    [MemberNotNullWhen(true, nameof(Names))]
    public bool HasNames => Names is not null;
    public static GhAssoc CreateFromDictionary(IDictionary dict, bool translateList = false)
    {
        var assoc = new GhAssoc(dict.Count);
        foreach (DictionaryEntry it in dict)
        {
            if (it.Key is not string name)
            {
                throw new ArgumentException("Key must be string.", nameof(dict));
            }

            if (it.Value is IDictionary id)
            {
                assoc.Add(name, CreateFromDictionary(id, translateList));
            }
            else
            {
                assoc.Add(name, it.Value);
            }
        }
        return assoc;
    }

    public override IEnumerable<string> GetNamesForExport()
    {
        if (Names is null) yield break;

        var index = 0;

        foreach (var it in Names)
        {
            yield return it ?? index.ToString(CultureInfo.InvariantCulture);
            ++index;
        }
    }

    internal override List<string?>? GetRawNames() => Names;

    public IEnumerable<KeyValuePair<string?, object?>> GetPairs()
    {
        if (!HasValues) yield break;

        for (var i = 0; i < Values.Count; i++)
        {
            yield return new(Names?[i], Values[i]);
        }
    }

    internal IEnumerable<KeyValuePair<string, GhAssocBase>> GetNodes()
    {
        if (!HasNames || !HasValues) yield break;

        for (var i = 0; i < Names.Count; i++)
        {
            var name = Names[i];
            if (Values[i] is GhAssocBase inode && name is not null)
                yield return new KeyValuePair<string, GhAssocBase>(name, inode);
        }
    }

    internal IEnumerable<KeyValuePair<string, object?>> GetAttributes()
    {
        if (!HasNames || !HasValues) yield break;

        for (var i = 0; i < Names.Count; i++)
        {
            var name = Names[i];
            if (Values[i] is not GhAssocBase && name != null)
                yield return new KeyValuePair<string, object?>(name, Values[i]);
        }
    }

    internal IEnumerable<KeyValuePair<string, object?>> GetNamedValues()
    {
        if (!HasNames || !HasValues) yield break;

        for (var i = 0; i < Names.Count; i++)
        {
            var name = Names[i];
            if (name is not null)
                yield return new KeyValuePair<string, object?>(name, Values[i]);
        }
    }

    internal IEnumerable<string> GetNodeNames()
    {
        if (!HasNames || !HasValues) yield break;

        for (var i = 0; i < Length; i++)
        {
            var name = Names[i];
            if (name is not null && Values[i] is GhAssocBase)
                yield return name;
        }
    }

    internal IEnumerable<string> GetAttributeNames()
    {
        if (!HasNames || !HasValues) yield break;

        for (var i = 0; i < Length; i++)
        {
            var name = Names[i];
            if (name is not null && Values[i] is not GhAssocBase)
                yield return name;
        }
    }
}
