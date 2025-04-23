using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.UI.Toolbar;
using GrasshopperIO;
using PancakeNextCore.GH;
using Eto.Drawing;
using PancakeNextCore.Attributes;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;

namespace PancakeNextCore.Components.Algorithm;

[IoId("b521157b-0ed0-4229-940a-7c9c2d9357ee")]
[ComponentCategory("misc")]
public sealed class pcCategorize : PancakeComponent<pcCategorize>, IPancakeLocalizable<pcCategorize>
{
    public pcCategorize() {}
    public pcCategorize(IReader reader) : base(reader) {}

    protected override void RegisterInputs()
    {
        AddParam("key", access: Access.Twig);
        AddParam("value", access: Access.Twig);
    }

    protected override void RegisterOutputs()
    {
        AddParam("keylist", access: Access.Twig);
        AddParam("valuetree", access: Access.Tree);
    }

    protected override void Process(IDataAccess access)
    {
        access.GetITwig(0, out var keyList);
        access.GetITwig(1, out var valList);

        if (keyList.LeafCount != valList.LeafCount)
        {
            access.AddError("Mismatched lists", Strings.KeyAndValueListMustHaveTheSameLength);
            return;
        }

        if (keyList.LeafCount == 0)
        {
            access.AddWarning("Empty input", Strings.TheListCannotBeEmpty);
            return;
        }

        if(keyList.NullCount > 0)
        {
            access.AddError("Wrong keys", Strings.KeyCannotBeNull);
            return;
        }

        ITwig keysOut;
        ITree valsOut;

        switch (keyList)
        {
            case Twig<int> listOfInts:
                FastSortKeyValues(listOfInts, valList, out var kInt, out valsOut, SortIfPossible);
                keysOut = kInt;
                break;
            case Twig<double> listOfDoubles:
                FastSortKeyValues(listOfDoubles, valList, out var kDouble, out valsOut, SortIfPossible);
                keysOut = kDouble;
                break;
            case Twig<string> listOfStrings:
                FastSortKeyValues(listOfStrings, valList, out var kString, out valsOut, SortIfPossible);
                keysOut = kString;
                break;
            case Twig<bool> listOfBooleans:
                FastSortKeyValues(listOfBooleans, valList, out var kBoolean, out valsOut, SortIfPossible);
                keysOut = kBoolean;
                break;
            default:
                SortKeyValuesFallback(keyList, valList, out keysOut, out valsOut, SortIfPossible);
                break;
        }

        access.SetTwig(0, keysOut);
        access.SetTree(1, valsOut);
    }

    private static void FastSortKeyValues<T>(Twig<T> keyList, ITwig valList, out Twig<T> keysOut, out ITree valsOut, bool sortIfPossible = true)
        where T : IComparable<T>, IEquatable<T>
    {
        var grps = keyList.Pears.Zip(valList.Pears, (k, v) => (k, v)).GroupBy(kv => kv.k, PearEqualityComparer<T>.Instance);
        if (sortIfPossible)
            grps = grps.OrderBy(kv => kv.Key.Item);

        var realizedGroups = grps.ToArray();
        var keys = realizedGroups.Select(kv => kv.Key).ToArray();

        keysOut = Garden.TwigFromPears(keys);
        valsOut = Garden.ITreeFromITwigs(realizedGroups.Select(x => Garden.ITwigFromPears(x.Select(y => y.v))));
    }

    private static void SortKeyValuesFallback(ITwig keyList, ITwig valList, out ITwig keysOut, out ITree valsOut, bool sortIfPossible = true)
    {
        var grps = keyList.Pears.Zip(valList.Pears, (k, v) => (k, v)).GroupBy(kv => kv.k, PearEqualityComparerGeneric.Instance);
        if (sortIfPossible)
            grps = grps.OrderBy(kv => kv.Key, PearComparerGeneric.Instance);

        var realizedGroups = grps.ToArray();
        var keys = realizedGroups.Select(kv => kv.Key).ToArray();

        keysOut = Garden.ITwigFromPears(keys);
        valsOut = Garden.ITreeFromITwigs(realizedGroups.Select(x => Garden.ITwigFromPears(x.Select(y => y.v))));
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

    public static string StaticLocalizedName => Strings.Categorize;

    public static string StaticLocalizedDescription => Strings.CategorizeValuesByKeys;
}