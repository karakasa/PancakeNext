using System;
using System.Collections.Generic;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;

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
        throw new NotImplementedException();

        /*object o = null;
        var depth = 0;
        DA.GetData(0, ref o);
        DA.GetData(1, ref depth);
        if (!(o is INodeQueryReadCapable inode))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.InputTypeNotSupported);
            return;
        }

        var names = new List<string>();
        var values = new List<object>();

        foreach (var it in NodeQuery.EnumerateNode(inode, depthLimit: depth))
        {
            names.Add(it.Key);
            values.Add(it.Value);
        }

        DA.SetDataList(0, names);
        DA.SetDataList(1, values);*/
    }
}