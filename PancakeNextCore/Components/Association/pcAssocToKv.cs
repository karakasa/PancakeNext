using System;
using System.Collections.Generic;
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;

namespace PancakeNextCore.Components.Association;

[IoId("{68D5751E-EFD3-41B5-96D0-C90466B2F58F}")]
[ComponentCategory("data", 1)]
public class pcAssocToKv : PancakeComponent<pcAssocToKv>, IPancakeLocalizable<pcAssocToKv>
{
    public pcAssocToKv() { }
    public pcAssocToKv(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.DeconstructAssociaitveArrayAsKeyValueList;
    public static string StaticLocalizedDescription => Strings.DeconstructAnAssociativeArrayIntoListOfKeyPathsAndValues;
    protected override void RegisterInputs()
    {
        AddParam<AssociationParameter>("assoc");
        AddParam("depth", 0);
    }
    protected override void RegisterOutputs()
    {
        AddParam<TextParameter>("paths", Access.Twig);
        AddParam("value", Access.Twig);
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out object o);
        access.GetItem(1, out int depth);
        if (o is not INodeQueryReadCapable inode)
        {
            access.AddError("Unsupported type", Strings.InputTypeNotSupported);
            return;
        }

        var names = new List<string>();
        var values = new List<IPear?>();

        foreach (var it in NodeQuery.EnumerateNode(inode, depthLimit: depth))
        {
            names.Add(it.Key);
            values.Add(it.Value);
        }

        access.SetTwig(0, Garden.TwigFromList(names));
        access.SetTwig(1, Garden.ITwigFromPears(values));
    }
}