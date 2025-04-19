using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcAssocToDatatable : PancakeComponent
{
    public override string LocalizedName => Strings.AssociativeArrayToDatatable;
    public override string LocalizedDescription => Strings.CreateADatatableASetOfHeadersAndValuesSoThatDataCanBeUsedByLunchBoxHumanUIEtcOrExportedOnlyNamedValueWillBeExtractedRNByDefaultEachColumnIsStoredAsOneBranchUseTheOptionInTheContextMenuToStoreEntriesAsBranches;
    protected override void RegisterInputs()
    {
        AddParam("assoc2", GH_ParamAccess.list);
    }
    protected override void RegisterOutputs()
    {
        AddParam<Param_String>("headers", GH_ParamAccess.list);
        AddParam("values", GH_ParamAccess.tree);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var inpList = new List<GhAssoc>();
        DA.GetDataList(0, inpList);

        var nameList = inpList
            .SelectMany(a => a.GetRawNames().Where(n => n != null))
            .Distinct().ToList();

        if (nameList.Count == 0)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                Strings.NoValidNamesAreFoundThisComponentWillOnlyExportNamedValues);
            return;
        }

        var dt = new DataTree<object>();
        object emptyValue = string.Empty;

        var index = 0;

        if (_flipOutput)
        {
            foreach (var it in inpList)
            {
                dt.AddRange(nameList.Select(name =>
                {
                    if (!it.TryGet(name, out var output))
                    {
                        output = emptyValue;
                    }
                    return output;
                }), new GH_Path(index));

                index++;
            }
        }
        else
        {
            foreach (var name in nameList)
            {
                dt.AddRange(inpList.Select(it =>
                {
                    if (!it.TryGet(name, out var output))
                        output = emptyValue;
                    return output;
                }), new GH_Path(index));

                index++;
            }
        }

        DA.SetDataList(0, nameList);
        DA.SetDataTree(1, dt);
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
            return ComponentIcon.Assoc2Table;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("fbe31cf6-81e9-41d4-86e5-f8a9d311dc60"); }
    }

    private bool _flipOutput = false;

    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
        Menu_AppendSeparator(menu);
        var item = Menu_AppendItem(menu, Strings.FlipTheOutput, MnuFlipOutput, true, _flipOutput);
        item.ToolTipText = Strings.ByDefaultEachEntryIsStoredAsOneBranchNotFlippedIfTheOutputIsFlippedEachColumnIsStoredAsOneBranch;
        base.AppendAdditionalMenuItems(menu);
    }

    private void MnuFlipOutput(object sender, EventArgs e)
    {
        _flipOutput = !_flipOutput;
        ExpireSolution(true);
    }

    private const string FlipOption = "FlipOutput";

    public override bool Read(GH_IReader reader)
    {
        reader.TryGetBoolean(FlipOption, ref _flipOutput);
        return base.Read(reader);
    }

    public override bool Write(GH_IWriter writer)
    {
        writer.SetBoolean(FlipOption, _flipOutput);
        return base.Write(writer);
    }
}