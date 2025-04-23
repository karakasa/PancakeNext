using Grasshopper2.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal readonly struct OptimizedConditionTester<T> where T : notnull, IEquatable<T>
{
    public readonly int Count;
    private readonly T? _singleValue;
    private readonly T[]? _values;

    public OptimizedConditionTester()
    {
        Count = 0;
    }

    public OptimizedConditionTester(T val)
    {
        Count = 1;
        _singleValue = val;
    }

    public OptimizedConditionTester(T[] vals)
    {
        switch (vals.Length)
        {
            case 0:
                Count = 0;
                break;
            case 1:
                Count = 1;
                _singleValue = vals[0];
                break;
            default:
                Count = vals.Length;
                _values = vals;
                break;
        }
    }
    public bool Contains(T val, bool emptyAsTrue)
    {
        if (Count == 0)
        {
            return emptyAsTrue;
        }

        if (Count == 1)
        {
            return val.Equals(_singleValue!);
        }

        foreach (var it in _values!)
            if (it.Equals(val))
                return true;

        return false;
    }
}
