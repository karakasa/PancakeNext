using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;
using Pancake.Interfaces;
using Pancake.Utility;
using Pancake.Utility.SimpleJsonSerializer;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcAssocToString : PancakeComponent, IPerformanceProfiler
{
    public override string LocalizedName => Strings.AssociativeArrayToString;
    public override string LocalizedDescription => Strings.ConvertAnAssociativeArrayToAStringOfCertainStyleSuchAsJsonPythonEtcRightClickTheComponentToChooseStyle;
    protected override void RegisterInputs()
    {
        AddParam("assoc");
    }
    protected override void RegisterOutputs()
    {
        AddParam<Param_String>("string");
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        object obj = null;
        DA.GetData(0, ref obj);

        if (obj == null)
            return;

        if (obj is GhAssoc assoc)
        {
            _profiler.Clear();
            _profiler.BeginStage("parse");
            var str = assoc.ToString(conversionType);
            _profiler.EndStage("parse");

            DA.SetData(0, str);
            return;
        }
        else if (obj is GhAtomList list)
        {
            _profiler.Clear();
            _profiler.BeginStage("parse");
            var str = GhAssoc.ListToJson(list, conversionType);
            _profiler.EndStage("parse");

            DA.SetData(0, str);
            return;
        }

        obj = GooHelper.UnwrapIfPossible(obj);

        if (obj is ISimpleJsonSerializable jsonObj && conversionType == GhAssoc.StringConversionType.Json)
        {
            var str = SimpleSerializer.Serialize(jsonObj);

            DA.SetData(0, str);
        }
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap LightModeIcon => ComponentIcon.Assoc2Str;

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("89e37b0a-e1c3-43e9-b4cb-1c33b6d20627"); }
    }

    private GhAssoc.StringConversionType conversionType = GhAssoc.StringConversionType.Json;

    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
        Menu_AppendSeparator(menu);
        AddStyleMenuItem(menu, "Json", GhAssoc.StringConversionType.Json);
        AddStyleMenuItem(menu, "HTTP", GhAssoc.StringConversionType.Querystring);
        AddStyleMenuItem(menu, "Python dict", GhAssoc.StringConversionType.Python);
        AddStyleMenuItem(menu, "Association", GhAssoc.StringConversionType.Association);
    }

    private void AddStyleMenuItem(ToolStripDropDown menu, string name,
        GhAssoc.StringConversionType style)
    {
        Menu_AppendItem(menu, name, (sender, e) =>
        {
            SetStyle(style);
            UpdateMessage();
            ExpireSolution(true);
        },
            true, conversionType == style);
    }

    private void SetStyle(GhAssoc.StringConversionType style)
    {
        conversionType = style;
        ExpireSolution(true);
    }

    private const string StyleOptionName = "Style";

    public override bool Write(GH_IWriter writer)
    {
        writer.SetString(StyleOptionName, conversionType.ToString());
        return base.Write(writer);
    }

    public override bool Read(GH_IReader reader)
    {
        string option = null;

        if (reader.TryGetString(StyleOptionName, ref option))
        {
            if (Enum.TryParse<GhAssoc.StringConversionType>(option, out var result))
            {
                conversionType = result;
            }
        }

        UpdateMessage();

        return base.Read(reader);
    }

    private void UpdateMessage()
    {
        Message = Enum.GetName(typeof(GhAssoc.StringConversionType), conversionType);
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

    private static readonly string[] InternalKeywords = new string[] { "json" };
    public override IEnumerable<string> LocalizedKeywords => InternalKeywords;
}