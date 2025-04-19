using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;
using Pancake.Utility;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcCsvToAssoc : PancakeComponent
{
    public override string LocalizedName => Strings.ConstructAssociativeArrayFromCSV;
    public override string LocalizedDescription => Strings.CreateAListOfAssociativeArraysFromCSVLinesSeeExampleForMoreInformation;
    protected override void RegisterInputs()
    {
        AddParam<Param_String>("csv", GH_ParamAccess.list);
        AddParam("delimiter", ",");
    }

    protected override void RegisterOutputs()
    {
        AddParam("assoc", GH_ParamAccess.list);
    }

    private bool headerless = false;

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var lines = new List<string>();
        string delimiter = null;

        DA.GetDataList(0, lines);
        DA.GetData(1, ref delimiter);

        if (lines.Count == 1)
        {
            lines[0] = FileIo.ReadContentIfIsFile(lines[0]);

            var splittedLines = lines[0].Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            lines.Clear();
            lines.AddRange(splittedLines);
        }

        lines.RemoveAll(l => l.Trim().Length == 0);

        if (string.IsNullOrEmpty(delimiter))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.DelimiterIsEmpty);
            return;
        }

        var delimiterArray = new[] { delimiter };

        var assocs = new List<GhAssoc>
        {
            Capacity = lines.Count
        };

        string[] headers = null;

        if (!headerless)
        {
            headers = GetHeaders(lines.First(), delimiterArray);
        }

        foreach (var it in lines.Skip(headerless ? 0 : 1))
        {
            var lineContent = it.Split(delimiterArray, StringSplitOptions.None);
            assocs.Add(ConvertLineToAssoc(headers, lineContent));
        }

        DA.SetDataList(0, assocs);
    }

    private string[] GetHeaders(string headerLine, string[] delimiterArray)
    {
        if (_recognizeFormat)
            return headerLine.Split(delimiterArray, StringSplitOptions.None)
                .Select(UnquoteString).ToArray();
        else
            return headerLine.Split(delimiterArray, StringSplitOptions.None);
    }

    private static string UnquoteString(string strData)
    {
        var str = strData.Trim();
        if (str.StartsWith("\"") && str.EndsWith("\""))
        {
            str = str.Substring(1, str.Length - 2);
        }
        return str;
    }

    private static string GetHeaderName(string[] headers, int index)
    {
        if (headers == null || index >= headers.Length || index < 0)
            return null;

        if (string.IsNullOrEmpty(headers[index]))
            return null;

        return headers[index];
    }

    private GhAssoc ConvertLineToAssoc(string[] headers, string[] content)
    {
        var assoc = new GhAssoc(content.Length);

        for (var i = 0; i < content.Length; i++)
        {
            var header = GetHeaderName(headers, i);
            var result = _recognizeFormat ? StringUtility.RecognizeType(content[i]) : content[i];
            if (header == null)
            {
                assoc.Add(result);
            }
            else
            {
                assoc.Add(header, result);
            }
        }

        return assoc;
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap LightModeIcon => ComponentIcon.CSV2Assoc;

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("badd27c5-4a24-48bc-802d-37992a027531"); }
    }

    private bool _recognizeFormat = true;
    private bool _parseAsQuantity = true;

    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
        Menu_AppendSeparator(menu);
        Menu_AppendItem(menu, Strings.RecognizeTypeIfPossible, MnuRecognizeType, true, _recognizeFormat);
        Menu_AppendItem(menu, Strings.ParseAsQuantityIfPossible, MnuParseAsQuantity, true, _parseAsQuantity);
    }

    private void MnuParseAsQuantity(object sender, EventArgs e)
    {
        _parseAsQuantity = !_parseAsQuantity;
        ExpireSolution(true);
    }

    private void MnuRecognizeType(object sender, EventArgs e)
    {
        _recognizeFormat = !_recognizeFormat;
        ExpireSolution(true);
    }

    private const string RecognizeTypeOption = "RecognizeType";
    private const string ParseAsQuantityOption = "ParseAsQuantity";

    public override bool Write(GH_IWriter writer)
    {
        writer.SetBoolean(RecognizeTypeOption, _recognizeFormat);
        writer.SetBoolean(ParseAsQuantityOption, _parseAsQuantity);
        return base.Write(writer);
    }

    public override bool Read(GH_IReader reader)
    {
        reader.TryGetBoolean(RecognizeTypeOption, ref _recognizeFormat);
        reader.TryGetBoolean(ParseAsQuantityOption, ref _parseAsQuantity);

        return base.Read(reader);
    }

    private static readonly List<string> InternalKeywords = new List<string>() { "csv" };
    public override IEnumerable<string> LocalizedKeywords => InternalKeywords;
}