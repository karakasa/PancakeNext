using Grasshopper2.Data.Meta.Summary;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public sealed class GH_Path : IComparable<GH_Path>, IComparer<GH_Path>, IEquatable<GH_Path>
{
    private int[] indices;

    public int this[int index]
    {
        get => indices[index];
        set => indices[index] = value;
    }

    public int[] Indices
    {
        get => indices;
        set => indices = value ?? [];
    }

    public int Length => indices?.Length ?? 0;

    public bool Valid
    {
        get
        {
            foreach (var index in indices)
                if (index < 0) return false;

            return indices.Length != 0;
        }
    }

    public GH_Path()
    {
        indices = [];
    }

    public GH_Path(int index)
    {
        indices = [index];
    }

    public GH_Path(params int[] args)
    {
        indices = ((args?.Length ?? 0) == 0) ? [] : ((int[])args.Clone());
    }

    public GH_Path(GH_Path Other) : this(Other.indices)
    {
    }

    public bool Equals(GH_Path? other) => this == other;
    public override bool Equals(object? obj) => obj is GH_Path another && Equals(another);

    public static bool operator ==(GH_Path? A, GH_Path? B)
    {
        if (A is null || B is null) return A is null && B is null;
        return A.indices.SequenceEqual(B.indices);
    }

    public static bool operator !=(GH_Path? A, GH_Path? B)
    {
        return !(A == B);
    }

    public static bool operator <(GH_Path? A, GH_Path? B)
    {
        int length = A.Length;
        int length2 = B.Length;

        if (length < length2)
        {
            if (length == 0)
            {
                return true;
            }
            int num = length - 1;
            for (int i = 0; i <= num; i++)
            {
                if (A.indices[i] < B.indices[i])
                {
                    return true;
                }
                if (A.indices[i] > B.indices[i])
                {
                    return false;
                }
            }
            return true;
        }
        if (length > length2)
        {
            if (length2 == 0)
            {
                return false;
            }
            int num2 = length2 - 1;
            for (int j = 0; j <= num2; j++)
            {
                if (A.indices[j] < B.indices[j])
                {
                    return true;
                }
                if (A.indices[j] > B.indices[j])
                {
                    return false;
                }
            }
            return false;
        }
        if (length == 0)
        {
            return false;
        }
        int num3 = length - 1;
        for (int k = 0; k <= num3; k++)
        {
            if (A.indices[k] < B.indices[k])
            {
                return true;
            }
            if (A.indices[k] > B.indices[k])
            {
                return false;
            }
        }
        return false;
    }

    public static bool operator >(GH_Path? A, GH_Path? B)
    {
        return A != B && !(A < B);
    }

    public override int GetHashCode()
    {
        if (indices is null) { return 0; }

        var sum = 0;
        unchecked
        {
            foreach (var i in indices)
            {
                sum *= -1000037;
                sum += i;
            }

            return sum;
        }
    }

    public int CompareTo(GH_Path? other)
    {
        if (this == other) return 0;
        return this < other ? -1 : 1;
    }

    public int Compare(GH_Path? x, GH_Path? y)
    {
        return x?.CompareTo(y) ?? -1;
    }

    public override string ToString()
    {
        return Format("{{{0}}}", ";");
    }

    public string ToString(bool includeBrackets)
    {
        return includeBrackets ? ToString() : Format("{0}", ";");
    }

    public string Format(string fString, string separator)
    {
        var innerStr = indices is null ? "null" : string.Join(separator, indices);
        return string.Format(fString, innerStr);
    }
    public GH_Path AppendElement(int index)
    {
        return new([..indices.Append(index)]);
    }
    public GH_Path PrependElement(int index)
    {
        return new([.. indices.Prepend(index)]);
    }
}