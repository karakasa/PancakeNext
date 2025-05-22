using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.GH.Params.AssocConverters;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;

namespace PancakeNextCore.Components.Association;

[IoId("{FE555B52-23B3-4589-8276-26CF7F9AED57}")]
[ComponentCategory("assoc", 1)]
public sealed class pcXmlToAssoc : PancakeComponent<pcXmlToAssoc>, IPancakeLocalizable<pcXmlToAssoc>
{
    public pcXmlToAssoc() { }
    public pcXmlToAssoc(IReader reader) : base(reader) { }

    protected override void ReadConfig()
    {
        _collapseAttributes = CustomValues.Get(CfgCollapseAttributes, false);
    }

    private bool _collapseAttributes;

    private const string CfgCollapseAttributes = "CollapseAttributes";
    public bool CollapseAttributes
    {
        get => _collapseAttributes;
        set => CustomValues.Set(CfgCollapseAttributes, _collapseAttributes = value);
    }
    protected override void RegisterInputs()
    {
        AddParam<TextParameter>("string");
        AddParam<TextParameter>("key2", access: Access.Twig, requirement: Requirement.MayBeMissing);
    }

    protected override void RegisterOutputs()
    {
        AddParam<QuantityParameter>("assoc");
        AddParam<TextParameter>("roottag");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out string content);
        access.GetTwig<string>(1, out var keys);

        try
        {
            var assoc = XmlIo.ReadXml(content, out var root, keys?.Items);

            if (CollapseAttributes)
                XmlIo.CollapseXmlAssoc(assoc);

            access.SetItem(0, assoc);
            access.SetItem(1, root);
        }
        catch (Exception e)
        {
            access.AddError("Parse failure", Strings.FailToParseXML + "\r\n" + e.ToString());
        }
    }

    protected override InputOption[][] SimpleOptions => [[
            new ToggleOption("Collapse inner content", "Simplify XML by collapsing single inner content of a tag into the tag itself.\r\nDo not use the option if you want to re-export the association to XML.",
                CollapseAttributes, x => CollapseAttributes = x, "Collapse attributes", "Keep as it is", "Collapse & Simplify")
            ]];

    public static string StaticLocalizedName => Strings.XMLToAssoc;

    public static string StaticLocalizedDescription => Strings.ReadXMLFileIntoAssocYouMayNeedAssocToKeyValuesToConvert;
    protected override IIcon? IconInternal => IconHost.CreateFromPathResource("XMLToAssoc");
}
