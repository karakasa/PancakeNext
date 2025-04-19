using Grasshopper.Kernel;
using Pancake.Attributes;
using Pancake.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Components.Association;

[ComponentCategory("data", 1)]
public class pcJsonToAssoc : PancakeComponent
{
    public override string LocalizedName => "Json to Assoc";

    public override string LocalizedDescription => "Converts a json string to Assoc object.\r\nUse 'Assoc to String' to convert assoc to json.";

    public override Guid ComponentGuid => new("{C92DF7F3-F383-483B-BFE7-3342788DF589}");

    protected override void RegisterInputs()
    {
        AddParam("json");
    }

    protected override void RegisterOutputs()
    {
        AddParam("assoc");
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        string str = default;
        DA.GetData(0, ref str);

        if (str.StartsWith("<"))
        {
            if (str.StartsWith("<?xml") || str.StartsWith("<? xml"))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input is XML content. Use 'XML to Assoc' instead.");
                return;
            }
        }

        if (JsonUtility.TryParseJsonLight(str, out var assoc))
        {
            DA.SetData(0, assoc);
        }
        else
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to parse. Be advised that Pancake may reject non-standard json formats.");
        }
    }

    protected override Bitmap LightModeIcon => ComponentIcon.StringToAssoc;
}
