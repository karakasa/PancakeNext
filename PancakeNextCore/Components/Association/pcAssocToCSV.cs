using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using Rhino.Commands;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 2)]
[IoId("fbe31cf6-81e9-41d4-86e5-f8a9d311dc61")]
public sealed class pcAssocToCsv : PancakeComponent<pcAssocToCsv>, IPancakeLocalizable<pcAssocToCsv>
{
    public pcAssocToCsv() { }
    public pcAssocToCsv(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.AssocToCSV;
    public static string StaticLocalizedDescription => Strings.ConvertANumberOfAssocsIntoCSVContentYouMayUseExportTXTComponentToWriteTheCSVContentIntoFile;
    protected override void RegisterInputs()
    {
        AddParam<AssociationParameter>("assoc2", Access.Twig);
        AddParam<TextParameter>("interestednames", Access.Twig, Requirement.MayBeMissing);
        AddParam("delimiter", ",");
    }
    protected override void RegisterOutputs()
    {
        AddParam<TextParameter>("csv");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetTwig<GhAssocBase>(0, out var inpList);
        access.GetTwig<string>(1, out var interestedNames);
        access.GetItem(2, out string delimiter);

        if (string.IsNullOrEmpty(delimiter))
        {
            access.AddError("Wrong delimiter", Strings.DelimiterCannotBeEmpty);
            return;
        }

        var lazyList = inpList.Items
            .SelectMany(a => a.GetRawNames().Where(n => n != null))
            .Distinct();

        var nameList = (interestedNames.LeafCount == 0 ? lazyList : lazyList.Intersect(interestedNames.Items)).ToArray();

        if (nameList.Length == 0)
        {
            access.AddError("No names",
                Strings.NoValidNamesAreFoundThisComponentWillOnlyExportNamedValues);
            return;
        }

        var emptyValue = string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(delimiter, nameList));

        foreach (var it in inpList.Items)
        {
            sb.AppendLine(string.Join(delimiter, nameList.Select(n =>
            {
                if (!TryGet(it, n, out var output))
                    return emptyValue;
                return output.ToString();
            })));
        }

        access.SetItem(0, sb.ToString());
    }

    private static bool TryGet(GhAssocBase it, string n, out object? output)
    {
        if (it is GhAssoc assoc)
        {
            var result = assoc.TryGet(n, out var pear);
            output = pear?.Item;
            return result;
        }

        output = null;
        return false;
    }
    protected override IIcon? IconInternal => IconHost.CreateFromPathResource("AssocToCsv");
}