using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using GrasshopperIO;
using PancakeNextCore.Polyfill;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
public abstract class GhAssocBase : IStorable, ICloneable
{
    private sealed class TemporaryPear : IPear
    {

        public Type Type => Item!.GetType();

        public object? Item { get; set; }

        public MetaData? Meta => null;
    }

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
    private static readonly Name IoLabelPrincipleVal = new("PrincipleVal");

    public GhAssocBase(IReader reader)
    {
        var count = reader.Integer32(IoLabelDataCount);
        if (count <= 0)
            return;

        EnsureData(count);

        _principleIndex = reader.TryRead(IoLabelPrincipleVal, -1);

        for (var i = 0; i < count; i++)
        {
            var location = new Name(IoLabelData, i);

            if (reader.FindReader(location) is { } nodeReader)
            {
                Values.Add(Garden.ReadPear(nodeReader).Item);
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

        if (_principleIndex >= 0)
            writer.Integer32(IoLabelPrincipleVal, _principleIndex);

        var tempPear = new TemporaryPear();

        for (var i = 0; i < length; i++)
        {
            var location = new Name(IoLabelData, i);

            var val = Values[i];

            if (val is null)
            {

            }
            else
            {
                tempPear.Item = val;
                Garden.WritePear(writer.CreateWriter(location), tempPear);
            }
        }
    }

    public int Length => Values is null ? 0 : Values.Count;
    public List<object?>? Values { get; protected set; }

    [MemberNotNullWhen(true, nameof(Values))]
    public bool HasValues => Values is not null;

    [MemberNotNull(nameof(Values))]
    protected virtual void EnsureData(int anticipatedCapacity = 0)
    {
        if (anticipatedCapacity > 0)
        {
            Values ??= new List<object?>(anticipatedCapacity);
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

    public object? Get(int index)
    {
        IsOutOfRange(index, throwOnError: true);
        return Values![index];
    }

    public void Set(int index, object? value)
    {
        IsOutOfRange(index, throwOnError: true);
        Values![index] = value;
    }

    public bool TryGet(int index, out object? output)
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
    public object? GetPrincipleValue()
    {
        if (!HasValues)
            throw new ArgumentException("The association is empty.");

        if (IsOutOfRange(_principleIndex))
            return Values[0];

        return Values[_principleIndex];
    }

    protected int _principleIndex = -1;
    public bool SetPrincipleValue(int index)
    {
        if (IsOutOfRange(index))
            return false;

        _principleIndex = index;
        return true;
    }
    public override int GetHashCode()
    {
        if (!HasValues) return 0;

        return unchecked(-940306134
            + EqualityComparer<List<object?>>.Default.GetHashCode(Values) * 13);
    }
    internal abstract GhAssocBase GenericClone();
    object ICloneable.Clone() => GenericClone();
    public abstract IEnumerable<string> GetNamesForExport();
    public virtual bool DeepEquals(GhAssocBase? another) => throw new NotSupportedException();
    internal virtual List<string?>? GetRawNames() => throw new NotSupportedException();
}
