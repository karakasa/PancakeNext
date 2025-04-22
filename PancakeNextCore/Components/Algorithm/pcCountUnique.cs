using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PancakeNextCore.Components.Algorithm;

[IoId("b521157b-0ed0-5229-940a-7c9c2d9357ee")]
[ComponentCategory("data", 0)]
public sealed class pcCountUnique : PancakeComponent<pcCountUnique>, IPancakeLocalizable<pcCountUnique>
{
    public pcCountUnique() { }
    public pcCountUnique(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.CountUnique;
    public static string StaticLocalizedDescription => Strings.ExtractAllUniqueValuesFromAListAndCountTheirOccurences;
    protected override void RegisterInputs()
    {
        AddParam("key", Access.Twig);
    }
    protected override void RegisterOutputs()
    {
        AddParam("keylist", Access.Twig);
        AddParam<IntegerParameter>("count", Access.Twig);
    }

    protected override void Process(IDataAccess access)
    {
        access.GetITwig(0, out var keyList);
        
        if (keyList.ItemCount == 0)
        {
            access.AddWarning("Empty input", Strings.TheListCannotBeEmpty);
            return;
        }

        if (keyList.NullCount > 0)
        {
            access.AddError("Wrong keys", Strings.KeyCannotBeNull);
            return;
        }

        ITwig keysOut;
        Twig<int> occurenceOut;

        switch (keyList)
        {
            case Twig<int> listOfInts:
                FastCountKeys(listOfInts, out var kInt, out occurenceOut, SortIfPossible);
                keysOut = kInt;
                break;
            case Twig<double> listOfDoubles:
                FastCountKeys(listOfDoubles, out var kDouble, out occurenceOut, SortIfPossible);
                keysOut = kDouble;
                break;
            case Twig<string> listOfStrings:
                FastCountKeys(listOfStrings, out var kString, out occurenceOut, SortIfPossible);
                keysOut = kString;
                break;
            case Twig<bool> listOfBooleans:
                FastCountKeys(listOfBooleans, out var kBoolean, out occurenceOut, SortIfPossible);
                keysOut = kBoolean;
                break;
            default:
                CountKeysFallback(keyList, out keysOut, out occurenceOut, SortIfPossible);
                break;
        }

        access.SetTwig(0, keysOut);
        access.SetTwig(1, occurenceOut);
    }

    private sealed class CountEntry<T>
    {
        public int Index { get; set; }
        public Pear<T>? Pear { get; set; }
        public int Count { get; set; }
    }

    private readonly struct CountEntryFactory : IStructFunc<IPear, CountEntry>
    {
        public CountEntry Invoke(IPear param) => new();
    }

    private sealed class CountEntry
    {
        public int Index { get; set; }
        public IPear? Pear { get; set; }
        public int Count { get; set; }
    }

    private readonly struct CountEntryFactory<T> : IStructFunc<T, CountEntry<T>>
    {
        public CountEntry<T> Invoke(T param) => new();
    }

    private static void FastCountKeys<T>(Twig<T> keyList, out Twig<T> keysOut, out Twig<int> countOut, bool sortIfPossible = true)
        where T : IComparable<T>, IEquatable<T>
    {
        var result = new StructFuncCache<CountEntryFactory<T>, T, CountEntry<T>>(default);

        var index = 0;
        foreach (var it in keyList.Pears)
        {
            var v = it.Item;
            var entry = result[v];
            if (entry.Pear is null)
            {
                entry.Index = index;
                entry.Pear = it;
                entry.Count = 1;
            }
            else
            {
                entry.Count++;
            }

            ++index;
        }

        KeyValuePair<T, CountEntry<T>>[] kvs = [ .. sortIfPossible
            ? result.InnerDictionary.OrderBy(kv => kv.Key)
            : result.InnerDictionary.OrderBy(kv => kv.Value.Index)];

        keysOut = Garden.TwigFromPears(kvs.Select(kv => kv.Value.Pear));
        countOut = Garden.TwigFromList(kvs.Select(kv => kv.Value.Count));
    }

    private static void CountKeysFallback(ITwig keyList, out ITwig keysOut, out Twig<int> countOut, bool sortIfPossible = true)
    {
        var result = new StructFuncCache<CountEntryFactory, IPear, CountEntry>(default, PearEqualityComparerGeneric.Instance);

        var index = 0;
        foreach (var it in keyList.Pears)
        {
            var entry = result[it];
            if (entry.Pear is null)
            {
                entry.Index = index;
                entry.Pear = it;
                entry.Count = 1;
            }
            else
            {
                entry.Count++;
            }

            ++index;
        }

        KeyValuePair<IPear, CountEntry>[] kvs = [ .. sortIfPossible
            ? result.InnerDictionary.OrderBy(kv => kv.Key)
            : result.InnerDictionary.OrderBy(kv => kv.Value.Index)];

        keysOut = Garden.ITwigFromPears(kvs.Select(kv => kv.Value.Pear));
        countOut = Garden.TwigFromList(kvs.Select(kv => kv.Value.Count));
    }

    private const string SortIfPossibleConfigName = "SortIfPossible";
    private bool _sortIfPossible;

    public bool SortIfPossible
    {
        get => _sortIfPossible;
        set => SetValue(SortIfPossibleConfigName, _sortIfPossible = value);
    }
    private void MnuSortIfPossible(bool newValue)
    {
        SortIfPossible = newValue;
        Expire();
        Document?.Solution?.DelayedExpire(this);
    }

    protected override InputOption[][] SimpleOptions => [[
            new ToggleOption("Sort if possible", "Controls if keys should be sorted", SortIfPossible, MnuSortIfPossible, "Sort keys", "Keep order of keys")
            {
                OnColor = OpenColor.Blue5,
                OffColor = OpenColor.Gray0
            }
        ]];

    protected override void ReadConfig()
    {
        _sortIfPossible = CustomValues.Get(SortIfPossibleConfigName, false);
    }
}