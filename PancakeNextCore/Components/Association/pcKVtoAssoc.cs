using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Pancake.Attributes;
using Pancake.GH.Params;
using Pancake.Interfaces;
using Pancake.Utility;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcKvToAssoc : PancakeComponent
{
    public override string LocalizedName => Strings.ConstructAssociativeArrayByKeys;
    public override string LocalizedDescription => Strings.ConstructOrAdjustAnAssociativeArrayByKeyAndValues;
    protected override void RegisterInputs()
    {
        AddParam("assoc4");
        AddParam<Param_String>("paths", GH_ParamAccess.list);
        AddParam("values", GH_ParamAccess.list);
        AddParam("delimiter2", "/");

        Params.Input[0].Optional = true;
    }

    protected override void RegisterOutputs()
    {
        AddParam("assoc");
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        object assoc = null;
        string delimiter = null;
        var paths = new List<string>();
        var values = new List<object>();

        DA.GetData(0, ref assoc);
        DA.GetData(3, ref delimiter);
        DA.GetDataList(1, paths);
        DA.GetDataList(2, values);

        var singlePath = paths.Count == 1;
        var singleValue = values.Count == 1;

        if (paths.Count != values.Count && !singlePath && !singleValue)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.LengthOfNamesMustBeEqualToThatOfValues);
            return;
        }

        if (paths.Count == 0)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, Strings.TheListCannotBeEmpty);
            return;
        }

        if (assoc != null)
        {
            switch (assoc)
            {
                case GhAssoc assoc2:
                    assoc = assoc2.Duplicate();
                    break;
                case GhAtomList list2:
                    assoc = list2.Duplicate();
                    break;
                default:
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Strings.InputTypeNotSupported);
                    return;
            }
        }
        else
        {
            assoc = new GhAssoc();
        }

        var inode = assoc as INodeQueryWriteCapable;
        var delimiterArray = new[] { delimiter };

        var addMode = AddMode;
        var count = Math.Max(paths.Count, values.Count);

        for (var i = 0; i < count; i++)
        {
            var activePath = singlePath ? paths[0] : paths[i];
            var activeValue = singleValue ? values[0] : values[i];

            var path = activePath.Split(delimiterArray, StringSplitOptions.None);
            if (path.Length == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(Strings.InvalidPathAt0, i));
                continue;
            }

            if (!NodeQuery.TrySetNodeValue(inode, path, activeValue, addMode))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(Strings.FailToSet0, activePath));
            }
        }

        DA.SetData(0, inode);
    }

    private const string ConfigAddMode = "AddMode";
    public bool AddMode
    {
        get => GetValue(ConfigAddMode, false);
        set => SetValue(ConfigAddMode, value);
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
            return ComponentIcon.ConAssocKV;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("1cec82d3-6efb-4abd-b3de-f6023eb8aeff"); }
    }

    private void UpdateMessage()
    {
        Message = AddMode ? Strings.Append : Strings.Modify;
    }

    public override void AddedToDocument(GH_Document document)
    {
        base.AddedToDocument(document);
        UpdateMessage();
    }

    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
        Menu_AppendSeparator(menu);
        Menu_AppendItem(menu, Strings.AppendMode, MnuAppendMode, true, AddMode).ToolTipText =
            Strings.WhenThereReDuplicatedEntriesAppendToTheAssocRatherThanModifySeeExampleForMoreInformation;

        base.AppendAdditionalMenuItems(menu);
    }

    private void MnuAppendMode(object sender, EventArgs e)
    {
        AddMode = !AddMode;
        UpdateMessage();
        ExpireSolution(true);
    }
}