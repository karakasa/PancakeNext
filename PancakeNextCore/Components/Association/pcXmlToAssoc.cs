using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.Interfaces;
using Pancake.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcXmlToAssoc : PancakeComponent, IPerformanceProfiler
{
    public override string LocalizedName => Strings.XMLToAssoc;

    public override string LocalizedDescription => Strings.ReadXMLFileIntoAssocYouMayNeedAssocToKeyValuesToConvert;

    public override Guid ComponentGuid => new Guid("{FE555B52-23B3-4589-8276-26CF7F9AED57}");

    private const string CfgCollapseAttributes = "CollapseAttributes";

    public bool CollapseAttributes
    {
        get => GetValue(CfgCollapseAttributes, false);
        set => SetValue(CfgCollapseAttributes, value);
    }
    protected override void RegisterInputs()
    {
        AddParam<Param_String>("string");
        AddParam<Param_String>("key2", GH_ParamAccess.list);
        LastAddedParameter.Optional = true;
    }

    protected override void RegisterOutputs()
    {
        AddParam("assoc");
        AddParam<Param_String>("roottag");
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        string content = null;
        var keys = new List<string>();
        DA.GetData(0, ref content);
        DA.GetDataList(1, keys);

        try
        {
            _profiler.Clear();
            _profiler.BeginStage("parse");
            var assoc = XmlIo.ReadXml(content, out var root, keys);

            if (CollapseAttributes)
                XmlIo.CollapseXmlAssoc(assoc);

            _profiler.EndStage("parse");

            DA.SetData(0, assoc);
            DA.SetData(1, root);

        }
        catch (Exception e)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.FailToParseXML + "\r\n" + e.ToString());
        }
        finally
        {
            _profiler.EndStage("parse");
        }
    }

    protected override Bitmap LightModeIcon => ComponentIcon.XML2Assoc;

    private static readonly List<string> InternalKeywords = new List<string>() { "xml" };
    public override IEnumerable<string> LocalizedKeywords => InternalKeywords;

    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
        Menu_AppendSeparator(menu);
        Menu_AppendItem(menu, Strings.CompactAttributes, (sender, e) =>
        {
            CollapseAttributes = !CollapseAttributes;
            UpdateMessage();
            ExpireSolution(true);
        }, true, CollapseAttributes).ToolTipText = Strings.FormattingMayBeLostRNIfEnabledXMLNodesWithTextContent;
        base.AppendAdditionalMenuItems(menu);
    }

    private void UpdateMessage()
    {
        Message = CollapseAttributes ? Strings.Compact : null;
    }

    public override void AddedToDocument(GH_Document document)
    {
        UpdateMessage();
        base.AddedToDocument(document);
    }

    private readonly StageProfiler _profiler = new StageProfiler();
    public long GetInputProcessingTime() => 0;
    public long GetOutputProcessingTime() => 0;

    public long GetCalculationTime() => _profiler.GetElapsedMillisecondsOrInvalid("parse");
}
