using Grasshopper2.Data;
using Grasshopper2.Data.Meta.Summary;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Path = Grasshopper2.Data.Path;

namespace OneCodeTwoVersions.Polyfill;
public sealed class GH_Path : IComparable<GH_Path>, IEquatable<GH_Path>
{
    private int[] _indices;

    public int this[int index]
    {
        get => _indices[index];
        set => _indices[index] = value;
    }

    public int[] Indices
    {
        get => _indices;
        set => _indices = value ?? [];
    }

    public int Length => _indices?.Length ?? 0;

    public bool Valid
    {
        get
        {
            foreach (var index in _indices)
                if (index < 0) return false;

            return _indices.Length != 0;
        }
    }

    public GH_Path()
    {
        _indices = [];
    }

    public GH_Path(int index)
    {
        _indices = [index];
    }

    public GH_Path(params int[] args)
    {
        _indices = ((args?.Length ?? 0) == 0) ? [] : ((int[])args.Clone());
    }

    public GH_Path(GH_Path Other) : this(Other._indices)
    {
    }

    public bool Equals(GH_Path? other) => this == other;
    public override bool Equals(object? obj) => obj is GH_Path another && Equals(another);

    public static bool operator ==(GH_Path? A, GH_Path? B)
    {
        if (A is null || B is null) return A is null && B is null;
        return Equals(A._indices, B._indices);
    }

    private static bool Equals(int[] a, int[] b)
    {
#if NET
        return a.AsSpan().SequenceEqual(b.AsSpan());
#else
        if (a.Length != b.Length) return false;
        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
#endif
    }

    public static bool operator !=(GH_Path? A, GH_Path? B)
    {
        return !(A == B);
    }

    public static bool operator <(GH_Path? A, GH_Path? B)
    {
        var lA = A.Length;
        var lB = B.Length;
        var sharedLength = Math.Min(lA, lB);

        if (sharedLength == 0) { return lA == 0 && lB != 0; }

        for (int i = 0; i < sharedLength; i++)
        {
            var iA = A._indices[i];
            var iB = B._indices[i];

            if (iA == iB) continue;
            return iA < iB;
        }

        return lA < lB;
    }

    public static bool operator >(GH_Path? A, GH_Path? B)
    {
        var lA = A.Length;
        var lB = B.Length;
        var sharedLength = Math.Min(lA, lB);

        if (sharedLength == 0) { return lB == 0 && lA != 0; }

        for (int i = 0; i < sharedLength; i++)
        {
            var iA = A._indices[i];
            var iB = B._indices[i];

            if (iA == iB) continue;
            return iA > iB;
        }

        return lA > lB;
    }

    public override int GetHashCode()
    {
        if (_indices is null) { return 0; }

        var sum = 0;
        unchecked
        {
            foreach (var i in _indices)
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
        var innerStr = _indices is null ? "null" : string.Join(separator, _indices);
        return string.Format(fString, innerStr);
    }
    public GH_Path AppendElement(int index)
    {
        return new([.._indices.Append(index)]);
    }
    public GH_Path PrependElement(int index)
    {
        return new([.. _indices.Prepend(index)]);
    }
    public Path To2() => new(_indices);
}