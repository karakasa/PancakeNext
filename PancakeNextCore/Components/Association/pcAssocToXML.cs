using System;
using System.Collections.Generic;
using System.Xml;
using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.GH.Params.AssocConverters;
using PancakeNextCore.Interfaces;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("assoc", 2)]
[IoId("{83649765-DCCB-4B68-9DF9-B05EEC44FDA2}")]
public class pcAssocToXml : PancakeComponent<pcAssocToXml>, IPancakeLocalizable<pcAssocToXml>
{
    public pcAssocToXml() { }
    public pcAssocToXml(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.AssocToXML;
    public static string StaticLocalizedDescription => Strings.ExportAnAssocToXMLSeeExampleForMoreInformation;
    protected override void RegisterInputs()
    {
        AddParam<QuantityParameter>("assoc");
        AddParam<TextParameter>("roottag");
    }
    protected override void RegisterOutputs()
    {
        AddParam<TextParameter>("string");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out GhAssocBase obj);

        if (obj is not GhAssoc assoc)
        {
            access.AddError("Format failure", Strings.InputMustBeAnAssociativeArray);
            return;
        }

        access.GetItem(1, out string root);

        try
        {
            var io = new XmlIo
            {
                Headless = Headless,
                Expand = Expand
            };

            var str = io.CreateXml(root, assoc);
            access.SetItem(0, str);
        }
        catch (XmlException format)
        {
            access.AddError("Format failure", format.Message);
        }
    }

    private const string ConfigHeadless = "Headless";
    private const string ConfigExpand = "Expand";

    private bool _headless;
    private bool _expand;

    protected override void ReadConfig()
    {
        _headless = GetValue(ConfigHeadless, false);
        _expand = GetValue(ConfigExpand, false);
    }
    public bool Headless
    {
        get => _headless;
        set => SetValue(ConfigHeadless, _headless = value);
    }

    public bool Expand
    {
        get => _expand;
        set => SetValue(ConfigExpand, _expand = value);
    }

    const string ChapterName = "XML Header";

    protected override InputOption[][] SimpleOptions => [
        [
            new ToggleOption("Omit header", Strings.ControlIfTheXMLHeaderIsOmitted, Headless, x => Headless = x, "Omit XML header", "Emit XML header")],[
            new ToggleOption("Expand attributes", Strings.ExpandAttributesToSubNodesSeeExampleForMoreInformation, Expand, x => Expand = x, "Expand attributes to subnodes", "Keep attributes"),
            ]];
}