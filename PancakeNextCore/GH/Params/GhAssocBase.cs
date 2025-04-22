using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using GrasshopperIO;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
public abstract class GhAssocBase : IStorable, ICloneable, INodeQueryReadCapable, INodeQueryWriteCapable
{
    public GhAssocBase()
    {
    }

    public GhAssocBase(int capacity)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        EnsureData(capacity);
    }

    private static readonly Name IoLabelDataCount = new("Count");
    private const string IoLabelData = "Data";

    public GhAssocBase(IReader reader)
    {
        var count = reader.Integer32(IoLabelDataCount);
        if (count <= 0)
            return;

        EnsureData(count);

        for (var i = 0; i < count; i++)
        {
            var location = new Name(IoLabelData, i);

            if (reader.FindReader(location) is { } nodeReader)
            {
                Values.Add(Garden.ReadPear(nodeReader));
            }
            else
            {
                Values.Add(null);
            }
        }
    }
    public virtual void Store(IWriter writer)
    {
        if (!HasValues)
        {
            writer.Integer32(IoLabelDataCount, 0);
            return;
        }

        var length = Length;
        writer.Integer32(IoLabelDataCount, length);

        for (var i = 0; i < length; i++)
        {
            var location = new Name(IoLabelData, i);

            var val = Values[i];

            if (val is null)
            {

            }
            else
            {
                Garden.WritePear(writer.CreateWriter(location), val);
            }
        }
    }

    public int Length => Values is null ? 0 : Values.Count;
    public List<IPear?>? Values { get; protected set; }

    [MemberNotNullWhen(true, nameof(Values))]
    public bool HasValues => Values is not null;

    [MemberNotNull(nameof(Values))]
    protected virtual void EnsureData(int anticipatedCapacity = 0)
    {
        if (anticipatedCapacity > 0)
        {
            Values ??= new(anticipatedCapacity);
        }
        else
        {
            Values ??= [];
        }
    }

    [MemberNotNullWhen(false, nameof(Values))]
    protected bool IsOutOfRange(int index, bool throwOnError = false)
    {
        if (!HasValues || index < 0 || index >= Length)
        {
            if (throwOnError)
                throw new IndexOutOfRangeException();

            return true;
        }

        return false;
    }

    public IPear? Get(int index)
    {
        IsOutOfRange(index, throwOnError: true);
        return Values![index];
    }

    public void Set(int index, IPear? value)
    {
        IsOutOfRange(index, throwOnError: true);
        Values![index] = value;
    }

    public void Set(int index, object? value)
    {
        Set(index, value.AsPear());
    }

    public bool TryGet(int index, out IPear? output)
    {
        if (IsOutOfRange(index))
        {
            output = null;
            return false;
        }

        output = Values[index];
        return true;
    }

    public virtual void Clear()
    {
        Values?.Clear();
        Values = null;
    }
    protected static string? ObjectToString(object? obj)
    {
        if (obj is null)
            return "null";
        return obj.ToString();
    }

    public override int GetHashCode()
    {
        if (!HasValues) return 0;

        return unchecked(-940306134
            + EqualityComparer<List<IPear?>>.Default.GetHashCode(Values) * 13);
    }
    internal abstract GhAssocBase GenericClone();
    object ICloneable.Clone() => GenericClone();
    public abstract IEnumerable<string> GetNamesForExport();
    public abstract bool DeepEquals(GhAssocBase? another);
    internal abstract List<string?>? GetRawNames();
    public abstract bool TryGetNode(string name, [NotNullWhen(true)] out INodeQueryReadCapable? node);
    public abstract bool TryGetContent(string attributeName, [NotNullWhen(true)] out IPear? content);
    public abstract IEnumerable<KeyValuePair<string, INodeQueryReadCapable?>> GetNodes();
    public abstract IEnumerable<KeyValuePair<string, IPear?>> GetAttributes();
    public abstract IEnumerable<string> GetNodeNames();
    public abstract IEnumerable<string> GetAttributeNames();
    public abstract bool TryGetNode(string name, [NotNullWhen(true)] out INodeQueryWriteCapable? node, bool createIfNotExist);
    public abstract bool SetContent(string attributeName, IPear? content);
    public abstract bool AddContent(string attributeName, IPear? content);
}
