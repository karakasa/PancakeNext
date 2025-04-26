using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
internal sealed class TwigFactory<T>
{
    private static readonly bool RequiresExternalNullInfo = IsNotNullable(typeof(T));
    private static bool IsNotNullable(Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return false;
        }

        return t.IsValueType;
    }

    private readonly int Capacity = 0;
    private List<T> _items;
    private List<MetaData?>? _metaDatas;
    private List<bool>? _isNulls;
    public TwigFactory()
    {
        _items = [];
    }
    public TwigFactory(int capacity)
    {
        Capacity = capacity;
        _items = new(capacity);
    }
    public Twig<T> Create()
    {
        AssertValid();
        return Garden.TwigFromList(_items, _metaDatas, _isNulls);
    }
    public void AddNull()
    {
        Add(default, null, true);
    }
    public void Add(T v, MetaData? meta = null, bool isNull = false)
    {
        if (_metaDatas is null)
        {
            if (meta is not null)
            {
                EnsureList(ref _metaDatas);
                _metaDatas!.Add(meta);
            }
        }
        else
        {
            _metaDatas.Add(meta);
        }

        if (RequiresExternalNullInfo)
        {
            if (_isNulls is null)
            {
                if (isNull)
                {
                    EnsureList(ref _isNulls);
                    _isNulls!.Add(true);
                }
            }
            else
            {
                _isNulls.Add(isNull);
            }
        }

        _items.Add(v);
    }

    private void AssertValid()
    {
        if (_metaDatas is not null && _metaDatas.Count != _items.Count)
        {
            throw new Exception("TwigFactory Error: Metadatas Length Mismatch");
        }

        if (RequiresExternalNullInfo)
        {
            if (_isNulls is not null && _isNulls.Count != _isNulls.Count)
            {
                throw new Exception("TwigFactory Error: IsNulls Length Mismatch");
            }
        }
        else
        {
            if (_isNulls is not null)
                throw new Exception("TwigFactory Error: IsNulls should be null for nullable types.");
        }
    }
    private void EnsureList<T>(ref List<T>? list)
    {
        if (list is null)
        {
            var cnt = _items.Count;
            list = new List<T>(Capacity == 0 ? cnt : Capacity);
            for (var i = 0; i < cnt; i++)
            {
                list.Add(default!);
            }
        }
    }
}
