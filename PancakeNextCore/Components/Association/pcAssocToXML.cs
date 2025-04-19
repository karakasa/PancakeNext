using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;
using Pancake.Interfaces;
using Pancake.Utility;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcAssocToXml : PancakeComponent, IPerformanceProfiler
{
    public override string LocalizedName => Strings.AssocToXML;
    public override string LocalizedDescription => Strings.ExportAnAssocToXMLSeeExampleForMoreInformation;
    protected override void RegisterInputs()
    {
        AddParam("assoc");
        AddParam<Param_String>("roottag");
    }
    protected override void RegisterOutputs()
    {
        AddParam<Param_String>("string");
    }

    private XmlIo _xmlIo = new XmlIo();
    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        object obj = null;
        DA.GetData(0, ref obj);

        if (!(obj is GhAssoc assoc))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.InputMustBeAnAssociativeArray);
            return;
        }

        string root = null;

        DA.GetData(1, ref root);

        try
        {
            _profiler.Clear();

            _xmlIo.Headless = Headless;
            _xmlIo.Expand = Expand;

            _profiler.BeginStage("parse");
            var str = _xmlIo.CreateXml(root, assoc);
            _profiler.EndStage("parse");
            DA.SetData(0, str);
        }
        catch (XmlException format)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, format.Message);
        }
        finally
        {
            _profiler.EndStage("parse");
        }
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap LightModeIcon => ComponentIcon.Assoc2XML;

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("{83649765-DCCB-4B68-9DF9-B05EEC44FDA2}"); }
    }

    private static readonly string[] InternalKeywords = new string[] { "xml" };
    public override IEnumerable<string> LocalizedKeywords => InternalKeywords;

    private const string ConfigHeadless = "Headless";
    private const string ConfigExpand = "Expand";
    public bool Headless
    {
        get => GetValue(ConfigHeadless, false);
        set => SetValue(ConfigHeadless, value);
    }

    public bool Expand
    {
        get => GetValue(ConfigExpand, false);
        set => SetValue(ConfigExpand, value);
    }
    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
        Menu_AppendSeparator(menu);
        Menu_AppendItem(menu, Strings.Headless, (sender, e) => { Headless = !Headless; ExpireSolution(true); }, true, Headless)
            .ToolTipText = Strings.ControlIfTheXMLHeaderIsOmitted;
        Menu_AppendItem(menu, Strings.ExpandAttributes, (sender, e) => { Expand = !Expand; UpdateMessage(); ExpireSolution(true); }, true, Expand)
            .ToolTipText = Strings.ExpandAttributesToSubNodesSeeExampleForMoreInformation;

        base.AppendAdditionalMenuItems(menu);
    }

    private readonly StageProfiler _profiler = new StageProfiler();
    public long GetInputProcessingTime() => 0;
    public long GetOutputProcessingTime() => 0;

    public long GetCalculationTime() => _profiler.GetElapsedMillisecondsOrInvalid("parse");

    public override void AddedToDocument(GH_Document document)
    {
        UpdateMessage();
        base.AddedToDocument(document);
    }
    private void UpdateMessage()
    {
        Message = Expand ? "Expand" : "";
    }
}