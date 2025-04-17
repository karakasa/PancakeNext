using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal sealed class ParameterizedCache<TParam, TKey, TValue>(TParam param, Func<TParam, TKey, TValue> retriever) where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _cache = [];
    public TValue this[TKey key]
    {
        get
        {
#if NET
            ref var item = ref CollectionsMarshal.GetValueRefOrAddDefault(_cache, key, out var exists);
            return exists ? item! : (item = retriever(param, key));
#else
            if (_cache.TryGetValue(key, out var v)) return v;
            return _cache[key] = retriever(param, key);
#endif
        }
    }
    public void Clear()
    {
        _cache.Clear();
    }

    public void Remove(TKey key)
    {
        _cache.Remove(key);
    }
}
