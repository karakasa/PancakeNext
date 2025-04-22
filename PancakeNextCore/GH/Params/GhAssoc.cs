using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using GrasshopperIO;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using PancakeNextCore.Utility.Polyfill;
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

namespace PancakeNextCore.GH.Params;

[IoId("22B79783-C674-4BC4-AFBA-014C94D727BD")]
public sealed class GhAssoc : GhAssocBase, INodeQueryReadCapable, INodeQueryWriteCapable
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
        Add(name, item.AsPear());
    }

    public void Add(string? name, IPear? item)
    {
        EnsureData();

        Names.Add(name);
        Values.Add(item.AsPear());
    }

    public bool TryGet(string? name, out IPear? output)
    {
        if (!HasValues)
        {
            output = null;
            return false;
        }

        for (var i = 0; i < Length; i++)
        {
            if (string.Equals(name, Names?[i]))
            {
                output = Values[i];
                return true;
            }
        }

        output = null;
        return false;
    }

    public bool TryGetContent(string name, out IPear? output)
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

        var realName = name.Substr(0, lastIndex);
        var index = 0;

        for (var i = 0; i < Length; i++)
        {
            if (StringSpanOperations.Equals(realName, Names[i]))
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
        Values = [.. list.AsPears()];

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

    private IEnumerable<string> GetItemNames()
    {
        if (!HasValues)
            yield break;

        for (var i = 0; i < Length; i++)
        {
            yield return (Names[i] != null ? $"{Names[i]}: " : "") + ObjectToString(Values[i]);
        }
    }

    public override string ToString()
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
            if (!OptimizedOperators.SameContentQ(Values![i], another.Values![i])) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        if (!HasValues) return 0;

        return unchecked(-940306134
            + EqualityComparer<List<string?>>.Default.GetHashCode(Names) * 7
            + EqualityComparer<List<IPear?>>.Default.GetHashCode(Values) * 13);
    }

    public static bool operator ==(GhAssoc assoc1, GhAssoc assoc2)
    {
        return assoc1.Equals(assoc2);
    }

    public static bool operator !=(GhAssoc assoc1, GhAssoc assoc2)
    {
        return !(assoc1 == assoc2);
    }

    public bool TryGetNode(string name, [NotNullWhen(true)] out INodeQueryReadCapable? node)
    {
        TryGetNode(name, out var node2, false);
        node = node2 as INodeQueryReadCapable;
        return node != null;
    }

    public bool TryGetNode(string name, [NotNullWhen(true)] out INodeQueryWriteCapable? node, bool createIfNotExist)
    {
        if (!TryGetContent(name, out var obj))
        {
            if (createIfNotExist)
            {
                var assoc2 = new GhAssoc();
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

    public bool SetContent(string attributeName, IPear? content)
    {
        if (!HasValues || !HasNames)
        {
            Add(attributeName, content);
            return true;
        }

        var index = Names.IndexOf(attributeName);
        if (index == -1)
        {
            Add(attributeName, content);
            return true;
        }

        Values[index] = content;
        return true;
    }

    public IEnumerable<KeyValuePair<string, INodeQueryReadCapable?>> GetNodes()
    {
        if (!HasValues || !HasNames) yield break;

        for (var i = 0; i < Names.Count; i++)
        {
            if (Values[i] is INodeQueryReadCapable inode)
                yield return new KeyValuePair<string, INodeQueryReadCapable?>(Names[i], inode);
        }
    }

    public IEnumerable<KeyValuePair<string, IPear?>> GetAttributes()
    {
        if (!HasValues || !HasNames) yield break;

        for (var i = 0; i < Names.Count; i++)
        {
            if (Values[i] is not INodeQueryReadCapable && Names[i] != null)
                yield return new KeyValuePair<string, IPear?>(Names[i], Values[i]);
        }
    }

    public IEnumerable<string> GetNodeNames()
    {
        if (!HasValues || !HasNames) yield break;

        for (var i = 0; i < Names.Count; i++)
        {
            if (Values[i] is INodeQueryReadCapable inode)
                yield return Names[i];
        }
    }

    public IEnumerable<string> GetAttributeNames()
    {
        if (!HasValues || !HasNames) yield break;

        for (var i = 0; i < Names.Count; i++)
        {
            if (Values[i] is not INodeQueryReadCapable && Names[i] != null)
                yield return Names[i];
        }
    }

    public IEnumerable<KeyValuePair<string, IPear?>> GetNamedValues()
    {
        if (!HasValues || !HasNames) yield break;

        for (var i = 0; i < Names.Count; i++)
        {
            if (Names[i] != null)
                yield return new KeyValuePair<string, IPear?>(Names[i], Values[i]);
        }
    }

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

    public IEnumerable<KeyValuePair<string?, IPear?>> GetPairs()
    {
        if (!HasValues) yield break;

        for (var i = 0; i < Values.Count; i++)
        {
            yield return new(Names?[i], Values[i]);
        }
    }

    bool INodeQueryWriteCapable.AddContent(string attributeName, IPear? content)
    {
        Add(attributeName, content);
        return true;
    }
}
