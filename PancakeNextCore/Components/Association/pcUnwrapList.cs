using System;
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 2)]
[IoId("0c4ec27b-fdb1-46e0-ae41-bc6f9fa1518a")]
public sealed class pcUnwrapList : PancakeComponent<pcUnwrapList>, IPancakeLocalizable<pcUnwrapList>
{
    public pcUnwrapList() { }
    public pcUnwrapList(IReader reader) : base(reader) { }

    public static string StaticLocalizedName => Strings.UnwrapList;
    public static string StaticLocalizedDescription => Strings.UnwrappedAnAtomListToItsOriginalContent;

    protected override void RegisterInputs()
    {
        AddParam<AssociationParameter>("atomlist");
    }

    protected override void RegisterOutputs()
    {
        AddParam("items", Access.Twig);
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out GhAssocBase input);

        if (input is not GhAtomList list)
        {
            access.AddError("Wrong input", Strings.InputTypeNotSupported);
            return;
        }

        access.SetTwig(0, Garden.ITwigFromPears(list.Values));
    }
}
