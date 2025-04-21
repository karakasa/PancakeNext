using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Path = Grasshopper2.Data.Path;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
[IoId("fbe31cf6-81e9-41d4-86e5-f8a9d311dc60")]
public sealed class pcAssocToDatatable : PancakeComponent<pcAssocToDatatable>, IPancakeLocalizable<pcAssocToDatatable>
{
    public pcAssocToDatatable() { }
    public pcAssocToDatatable(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.AssociativeArrayToDatatable;
    public static string StaticLocalizedDescription => Strings.CreateADatatableASetOfHeadersAndValuesSoThatDataCanBeUsedByLunchBoxHumanUIEtcOrExportedOnlyNamedValueWillBeExtractedRNByDefaultEachColumnIsStoredAsOneBranchUseTheOptionInTheContextMenuToStoreEntriesAsBranches;
    protected override void RegisterInputs()
    {
        AddParam<AssociationParameter>("assoc2", Access.Twig);
    }
    protected override void RegisterOutputs()
    {
        AddParam<TextParameter>("headers", Access.Twig);
        AddParam("values", Access.Tree);
    }

    protected override void Process(IDataAccess access)
    {
        access.GetTwig<GhAssocBase>(0, out var inpList);

        var nameList = inpList.Items
            .SelectMany(a => a.GetRawNames().Where(n => n != null))
            .Distinct().ToList();

        if (nameList.Count == 0)
        {
            access.AddError("No names", Strings.NoValidNamesAreFoundThisComponentWillOnlyExportNamedValues);
            return;
        }

        var dt = new List<(IPear?[], Path)>();
        IPear? emptyValue = null;

        var index = 0;

        if (_flipOutput)
        {
            foreach (var it in inpList.Items)
            {
                dt.Add(([.. nameList.Select(name => TryGet(it, name, out var output) ? output : emptyValue)], new Path(index)));

                index++;
            }
        }
        else
        {
            foreach (var name in nameList)
            {
                dt.Add(([..inpList.Items.Select(it => TryGet(it, name, out var output) ? output : emptyValue)], new Path(index)));

                index++;
            }
        }

        access.SetTwig(0, Garden.TwigFromList(nameList));
        access.SetTree(1, Garden.ITreeFromITwigs(new Paths(dt.Select(x => x.Item2)), dt.Select(x => Garden.ITwigFromPears(x.Item1))));
    }

    private static bool TryGet(GhAssocBase it, string? n, out IPear? output)
    {
        if (it is GhAssoc assoc)
        {
            return assoc.TryGet(n, out output);
        }

        output = null;
        return false;
    }

    private bool _flipOutput = false;
    private const string FlipOption = "FlipOutput";

    public bool FlipOutput
    {
        get => _flipOutput;
        set => SetValue(FlipOption, _flipOutput = value);
    }

    protected override void ReadConfig()
    {
        _flipOutput = GetValue(FlipOption, false);
    }

    protected override InputOption[][] SimpleOptions => [[
            new ToggleOption(Strings.FlipTheOutput, Strings.ByDefaultEachEntryIsStoredAsOneBranchNotFlippedIfTheOutputIsFlippedEachColumnIsStoredAsOneBranch,
                FlipOutput, x => FlipOutput = x, "1 Column 1 Twig", "1 Item 1 Twig", "Flip"){
                OnColor = OpenColor.Blue7
            }
            ]];
}