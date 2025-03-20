using GrasshopperIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataType;

[IoId("22B79783-C674-4BC4-AFBA-014C94D727BD")]
public sealed class Association : IStorable
{
    public Association()
    {
    }
    public Association(IReader reader)
    {
    }
    public void Store(IWriter writer)
    {
        throw new NotImplementedException();
    }

    public int Length => Values is null ? 0 : Values.Count;

    internal List<string?>? Names { get; private set; }
    internal List<object>? Values { get; private set; }

    internal bool IsValid => Names is null || Names?.Count == Values?.Count;

    public void MergeWith(Association? another)
    {
        if (another is null)
        {
            throw new NullReferenceException(nameof(another));
        }

        if (!another.IsValid)
        {
            throw new ArgumentException(nameof(another));
        }

        var length = Values.Count;

        Values.AddRange(another.Values);

        _names.AddRange(another._names);
        for (var i = 0; i < another._names.Count; i++)
        {
            SetNameReference(another._names[i], length + i);
        }
    }

    public void Add(object item)
    {
        Add(null, item);
    }

    private void FillWithNullNames()
    {
        if (Names is not null) throw new InvalidOperationException(nameof(FillWithNullNames));
        if (Values is null) return;

        Names = new(Values.Count);
        for (var i = 0; i < Values.Count; i++)
            Names.Add(null);
    }

    public void Add(string? name, object item)
    {
        if (name is null)
        {
            Names?.Add(name);
        }
        else
        {
            if (Names is null)
            {
                FillWithNullNames();
            }

            Names ??= [];
            Names.Add(name);
        }

        Values ??= [];
        Values.Add(item);
    }

    public object Get(int index)
    {
        if (index < 0 || index > Length)
        {
            throw new IndexOutOfRangeException();
        }

        return Values[index];
    }

    public void Set(int index, object T)
    {
        if (index < 0 || index > Length)
        {
            throw new IndexOutOfRangeException();
        }

        Values[index] = T;
    }

    public bool TryGet(int index, out object output)
    {
        if (index < 0 || index >= Length)
        {
            output = null;
            return false;
        }
        output = Values[index];
        return true;
    }

    public bool TryGet(string name, out object output)
    {
        for (var i = 0; i < Length; i++)
        {
            if (_names[i] != null && _names[i].Equals(name))
            {
                output = Values[i];
                return true;
            }
        }

        output = null;
        return false;
    }

    private bool TryGetExtended(string name, out object output)
    {
        if (TryGet(name, out output))
            return true;

        if (name.StartsWith("@") && int.TryParse(name.Substring(1), out var indice))
        {
            if (indice >= 0 && indice < Values.Count)
            {
                output = Values[indice];
                return true;
            }
        }

        var lastIndex = name.LastIndexOf("@");
        if (lastIndex == -1 || !int.TryParse(name.Substring(lastIndex + 1), out indice))
        {
            output = null;
            return false;
        }

        var realName = name.Substring(0, lastIndex);
        var index = 0;

        for (var i = 0; i < Length; i++)
        {
            if (_names[i] != null && _names[i].Equals(realName))
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

    public IEnumerable<string> GetRawNames()
    {
        return _names.AsReadOnly();
    }

    public IEnumerable<string> GetNames()
    {
        var index = 0;

        foreach (var it in _names)
        {
            yield return it ?? index.ToString();
            ++index;
        }
    }

    public IEnumerable<string> GetJsonNames()
    {
        var index = 0;

        foreach (var it in _names)
        {
            yield return it ?? "item" + index.ToString();
            ++index;
        }
    }

    /// <summary>
    /// Create a tuple from a list
    /// </summary>
    /// <param name="list">List to be copied</param>
    public Association(IEnumerable<object> list)
    {
        Values = new List<object>(list);
        _names = new List<string>();
        _names.Capacity = Values.Count;
        Values.ForEach(u => _names.Add(null));
    }

    public Association(IEnumerable<string> names, IEnumerable<object> list)
    {
        Values = new List<object>(list);
        _names = new List<string>(names);

        if (Values.Count != _names.Count)
        {
            Values.Clear();
            _names.Clear();
            throw new ArgumentException();
        }
    }

    public override IGH_Goo Duplicate()
    {
        var tuple = new Association();
        tuple.MergeWith(this);

        return tuple;
    }

    public override string ToString()
    {
        if (_principleIndex == 0 && Values[0] is string str)
            return str;

        return GetDescriptiveString();
    }

    private static string ObjectToString(object obj)
    {
        if (obj == null)
            return "null";
        return obj.ToString();
    }

    private IEnumerable<string> GetItemNames()
    {
        for (var i = 0; i < Length; i++)
        {
            yield return ((_names[i] != null) ? $"{_names[i]}: " : "") + ObjectToString(Values[i]);
        }
    }

    public string GetDescriptiveString()
    {
        if (Values == null)
            return Strings.EmptyTuple;

        var types = string.Join(", ", GetItemNames());
        return $"<{types}>";
    }

    public object GetPrincipleValue()
    {
        if (_principleIndex < 0 || _principleIndex >= Values.Count)
            return Values[0];

        return Values[_principleIndex];
    }

    private int _principleIndex = -1;

    public bool SetPrincipleValue(int index)
    {
        if (index < 0 || index >= Values.Count)
            return false;

        _principleIndex = index;
        return true;
    }

    public bool SetPrincipleValue(string name)
    {
        if (!_nameIndexes.TryGetValue(name, out var index))
            return false;

        return SetPrincipleValue(index);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(obj, this))
            return true;

        if (obj is not Association assoc)
            return false;

        return Equals(assoc);
    }

    public void Clear()
    {
        _nameIndexes.Clear();
        _names.Clear();
        Values.Clear();
    }

    public bool Equals(Association assoc)
    {
        if (assoc is null) return false;

        if (assoc._names.Count != _names.Count || assoc.Values.Count != Values.Count)
            return false;

        if (_names.Any(n => n == null) || assoc._names.Any(n => n == null))
        {
            // Strict mode, value and order must be the same
            return !Value.Zip(assoc.Value, Equals).Any(r => r == false) &&
                !_names.Zip(assoc._names, string.Equals).Any(r => r == false);
        }
        else
        {
            for (var i = 0; i < _names.Count; i++)
            {
                var n = _names[i];
                object o = null;
                if (!assoc.TryGet(n, out o)) return false;
                if (!Equals(Values[i], o)) return false;
            }
            return true;
        }
    }

    public override int GetHashCode()
    {
        return -940306134
            + EqualityComparer<List<string>>.Default.GetHashCode(_names)
            + EqualityComparer<List<object>>.Default.GetHashCode(Values);
    }

    public static bool operator ==(Association assoc1, Association assoc2)
    {
        return EqualityComparer<Association>.Default.Equals(assoc1, assoc2);
    }

    public static bool operator !=(Association assoc1, Association assoc2)
    {
        return !(assoc1 == assoc2);
    }

    public static string ToString(object pancakeObj, StringConversionType style)
    {
        if (pancakeObj == null)
            return null;

        if (pancakeObj is GhAtomList list)
        {
            return ListToJson(list, style);
        }

        if (MetahopperWrapper.IsMetaHopperWrapper(pancakeObj))
        {
            try
            {
                var listObj = new GhAtomList(MetahopperWrapper.ExtractMetaHopperWrapper(pancakeObj));
                return ListToJson(listObj, style);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        if (pancakeObj is Association assoc)
        {
            return assoc.ToString(style);
        }

        return null;
    }

    public static bool IsSerializable(object obj)
    {
        return (!(obj == null)) && ((obj is Association) ||
            (obj is GhAtomList) || MetahopperWrapper.IsMetaHopperWrapper(obj));
    }

    internal static object DuplicateGeneral(object pancakeObj)
    {
        if (pancakeObj == null)
            return null;

        if (pancakeObj is GhAtomList list)
        {
            return list.Duplicate();
        }

        if (MetahopperWrapper.IsMetaHopperWrapper(pancakeObj))
        {
            try
            {
                var listObj = new GhAtomList(MetahopperWrapper.ExtractMetaHopperWrapper(pancakeObj));
                return listObj;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        if (pancakeObj is Association assoc)
        {
            return assoc.Duplicate();
        }

        return null;
    }

    public bool TryGetNode(string name, out INodeQueryReadCapable node)
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
    }

    public IEnumerable<string> GetNodeNames()
    {
        for (var i = 0; i < _names.Count; i++)
        {
            if (_names[i] != null && (Values[i] is INodeQueryReadCapable inode))
                yield return _names[i];
        }
    }

    public IEnumerable<string> GetAttributeNames()
    {
        for (var i = 0; i < _names.Count; i++)
        {
            if (_names[i] != null && !(Values[i] is INodeQueryReadCapable))
                yield return _names[i];
        }
    }

    public override object ScriptVariable()
    {
        return this;
    }

    private const string IoLabelDataCount = "Count";
    private const string IoLabelName = "Name";
    private const string IoLabelData = "Data";
    private const string IoLabelPrincipleVal = "PrincipleVal";

    public override bool Read(GH_IReader reader)
    {
        try
        {
            var count = reader.GetInt32(IoLabelDataCount);
            _principleIndex = reader.GetInt32(IoLabelPrincipleVal);

            Clear();
            _names.Capacity = Values.Capacity = count;
            for (var i = 0; i < count; i++)
            {
                string name = null;
                if (reader.TryGetString(IoLabelName, i, ref name))
                    _names.Add(name);
                else
                    _names.Add(null);

                Values.Add(GooHelper.ReadObject(reader.FindChunk(IoLabelData, i)));
            }
        }
        catch (Exception ex)
        {

        }
        return base.Read(reader);
    }

    public override bool Write(GH_IWriter writer)
    {
        try
        {
            writer.SetInt32(IoLabelDataCount, Values.Count);
            for (var i = 0; i < Values.Count; i++)
            {
                var chunk = writer.CreateChunk(IoLabelData, i);
                if (_names[i] != null)
                    chunk.SetString(IoLabelName, _names[i]);
                GooHelper.WriteObject(chunk, Values[i]);
            }
            writer.SetInt32(IoLabelPrincipleVal, _principleIndex);
        }
        catch (Exception e)
        {
            return false;
        }

        return base.Write(writer);
    }

    protected Association(SerializationInfo reader, StreamingContext context)
    {
        var count = reader.GetInt32(IoLabelDataCount);
        _principleIndex = reader.GetInt32(IoLabelPrincipleVal);

        Values = new List<object>();
        _names = new List<string>();

        _names.Capacity = Values.Capacity = count;
        for (var i = 0; i < count; i++)
        {
            var tag = i.ToString();
            var str = reader.GetString(IoLabelName + tag);

            _names.Add(str == "" ? null : str);
            Values.Add(GooHelper.ReadObject(reader, tag));
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(IoLabelDataCount, Values.Count);
        info.AddValue(IoLabelPrincipleVal, _principleIndex);
        for (var i = 0; i < Values.Count; i++)
        {
            var tag = i.ToString();
            info.AddValue(IoLabelName + tag, _names[i] ?? "");
            GooHelper.WriteObject(info, tag, Values[i]);
        }
    }

    public static Association CreateFromDictionary(IDictionary dict, bool translateList = false)
    {
        var assoc = new Association(dict.Count);
        foreach (DictionaryEntry it in dict)
        {
            if (!(it.Key is string name))
            {
                throw new ArgumentException(nameof(dict));
            }

            if (it.Value is GhAtomList || it.Value is Association)
            {
                assoc.Add(name, it.Value);
                continue;
            }

            if (it.Value is IDictionary id)
            {
                assoc.Add(name, CreateFromDictionary(id, translateList));
            }
            else
            {
                if (translateList && it.Value is IList il)
                {
                    assoc.Add(name, GhAtomList.CreateFromList(il));
                }
                else
                {
                    assoc.Add(name, it.Value);
                }
            }
        }
        return assoc;
    }

    public bool AddContent(string attributeName, object content)
    {
        Add(attributeName, content);
        return true;
    }
}
