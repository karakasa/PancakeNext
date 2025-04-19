using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcAssocToCsv : PancakeComponent
{
    public override string LocalizedName => Strings.AssocToCSV;
    public override string LocalizedDescription => Strings.ConvertANumberOfAssocsIntoCSVContentYouMayUseExportTXTComponentToWriteTheCSVContentIntoFile;
    protected override void RegisterInputs()
    {
        AddParam("assoc2", GH_ParamAccess.list);
        AddParam<Param_String>("interestednames", GH_ParamAccess.list);
        LastAddedParameter.Optional = true;
        AddParam("delimiter", ",", GH_ParamAccess.item);
    }
    protected override void RegisterOutputs()
    {
        AddParam<Param_String>("csv", GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var inpList = new List<GhAssoc>();
        var interestedNames = new List<string>();
        var delimiter = ",";
        DA.GetDataList(0, inpList);
        DA.GetDataList(1, interestedNames);
        DA.GetData(2, ref delimiter);

        if (string.IsNullOrEmpty(delimiter))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.DelimiterCannotBeEmpty);
            return;
        }

        var lazyList = inpList
            .SelectMany(a => a.GetRawNames().Where(n => n != null))
            .Distinct();

        var nameList = (interestedNames.Count == 0 ? lazyList : lazyList.Intersect(interestedNames)).ToArray();

        if (nameList.Length == 0)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                Strings.NoValidNamesAreFoundThisComponentWillOnlyExportNamedValues);
            return;
        }

        var dt = new DataTree<object>();
        var emptyValue = string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(delimiter, nameList));

        foreach (var it in inpList)
        {
            sb.AppendLine(string.Join(delimiter, nameList.Select(n =>
            {
                if (!it.TryGet(n, out var output))
                    return emptyValue;
                return output.ToString();
            })));
        }

        DA.SetData(0, sb.ToString());
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap LightModeIcon
    {
        get
        {
            //You can add image files to your project resources and access them like this:
            // return Resources.IconForThisComponent;
            return ComponentIcon.Assoc2CSV;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("fbe31cf6-81e9-41d4-86e5-f8a9d311dc61"); }
    }
}