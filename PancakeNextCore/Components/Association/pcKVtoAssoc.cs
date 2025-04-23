using System;
using System.Collections.Generic;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
[IoId("1cec82d3-6efb-4abd-b3de-f6023eb8aeff")]
public sealed class pcKvToAssoc : PancakeComponent<pcKvToAssoc>, IPancakeLocalizable<pcKvToAssoc>
{
    public pcKvToAssoc() { }
    public pcKvToAssoc(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.ConstructAssociativeArrayByKeys;
    public static string StaticLocalizedDescription => Strings.ConstructOrAdjustAnAssociativeArrayByKeyAndValues;
    protected override void RegisterInputs()
    {
        AddParam<AssociationParameter>("assoc4", requirement: Requirement.MayBeMissing);
        AddParam<TextParameter>("paths", Access.Twig);
        AddParam("values", Access.Twig);
        AddParam("delimiter2", "/");
    }

    protected override void RegisterOutputs()
    {
        AddParam<AssociationParameter>("assoc");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out object? assoc);
        access.GetItem(3, out string delimiter);
        access.GetTwig<string>(1, out var paths);
        access.GetITwig(2, out var values);

        var singlePath = paths.LeafCount == 1;
        var singleValue = values.LeafCount == 1;

        if (paths.LeafCount != values.LeafCount && !singlePath && !singleValue)
        {
            access.AddError("Length mismatch", Strings.LengthOfNamesMustBeEqualToThatOfValues);
            return;
        }

        if (paths.LeafCount == 0)
        {
            access.AddWarning("Empty input", Strings.TheListCannotBeEmpty);
            return;
        }

        if (assoc != null)
        {
            switch (assoc)
            {
                case GhAssoc assoc2:
                    assoc = assoc2.Clone();
                    break;
                case GhAtomList list2:
                    assoc = list2.GenericClone();
                    break;
                default:
                    access.AddWarning("Wrong input", Strings.InputTypeNotSupported);
                    return;
            }
        }
        else
        {
            assoc = new GhAssoc();
        }

        var inode = assoc as INodeQueryWriteCapable;
        var delimiterArray = new[] { delimiter };

        var addMode = AddMode;
        var count = Math.Max(paths.LeafCount, values.LeafCount);

        for (var i = 0; i < count; i++)
        {
            var activePath = singlePath ? paths[0] : paths[i];
            var activeValue = singleValue ? values[0] : values[i];

            var path = activePath.Item.Split(delimiterArray, StringSplitOptions.None);
            if (path.Length == 0)
            {
                access.AddWarning("Wrong path", string.Format(Strings.InvalidPathAt0, i));
                continue;
            }

            if (!NodeQuery.TrySetNodeValue(inode, path, activeValue, addMode))
            {
                access.AddWarning("Set failure", string.Format(Strings.FailToSet0, activePath));
            }
        }

        access.SetItem(0, inode);
    }

    private bool _addMode = false;
    private const string ConfigAddMode = "AddMode";

    protected override void ReadConfig()
    {
        _addMode = GetValue(ConfigAddMode, false);
    }
    public bool AddMode
    {
        get => _addMode;
        set => SetValue(ConfigAddMode, _addMode = value);
    }

    protected override InputOption[][] SimpleOptions => [[
            new ToggleOption(Strings.AppendMode, Strings.WhenThereReDuplicatedEntriesAppendToTheAssocRatherThanModifySeeExampleForMoreInformation,
                AddMode, x => AddMode = x, "Append", "Overwrite", "If keys already exists")
            ]];
}