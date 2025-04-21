using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal sealed class StructFuncCache<TFunc, TKey, TValue> 
    where TKey : notnull
    where TFunc : struct, IStructFunc<TKey, TValue>
{
    private readonly TFunc initializer;
    public Dictionary<TKey, TValue> InnerDictionary { get; }
    public StructFuncCache(TFunc func)
    {
        this.initializer = func;
        InnerDictionary = [];
    }

    public StructFuncCache(TFunc func, IEqualityComparer<TKey> comparer)
    {
        this.initializer = func;
        InnerDictionary = new(comparer);
    }

    public TValue this[TKey key]
    {
        get
        {
#if NET
            ref var item = ref CollectionsMarshal.GetValueRefOrAddDefault(InnerDictionary, key, out var exists);
            return exists ? item! : (item = initializer.Invoke(key));
#else
            if (InnerDictionary.TryGetValue(key, out var v)) return v;
            return InnerDictionary[key] = initializer.Invoke(key);
#endif
        }
    }
    public void Clear()
    {
        InnerDictionary.Clear();
    }

    public void Remove(TKey key)
    {
        InnerDictionary.Remove(key);
    }
}
