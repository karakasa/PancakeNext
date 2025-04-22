using System;
using System.Collections.Generic;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 0)]
[IoId("4c770bd7-149b-42ef-85e9-81a91f7c07b5")]
public sealed class pcWrapList : PancakeComponent<pcWrapList>, IPancakeLocalizable<pcWrapList>
{
    public pcWrapList() { }
    public pcWrapList(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.WrapList;
    public static string StaticLocalizedDescription => Strings.WrapAListToAnAtomListWhichIsAListButBeingTreatedAsOneSingleElement;
    protected override void RegisterInputs()
    {
        AddParam("items2", Access.Twig);
    }
    protected override void RegisterOutputs()
    {
        AddParam<AssociationParameter>("atomlist2");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetITwig(0, out var list);
        access.SetItem(0, new GhAtomList(list));
    }
}