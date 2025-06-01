using Grasshopper.Kernel;
using GrasshopperIO;
using OneCodeTwoVersions.Polyfill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions;
[IoId("BF647C43-8B2F-49C6-84D3-708BCAF2F133")]
public sealed class DemoCompWithCustomData : ComponentMiddleware<DemoCompWithCustomData>
{
    public DemoCompWithCustomData() : base("CustomData", "CustomData", "Demo component to manipulate custom data for Grasshopper 1 and 2", "Demo", "Demo")
    {
    }

    public DemoCompWithCustomData(IReader reader) : base(reader)
    {
    }
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new ParamCustomDataType(), "Custom", "CNP", "Creates a new object if the input is null. Otherwise a manipulated one is returned.", GH_ParamAccess.item);
        pManager.AddIntegerParameter("V1", "V1", "Value 1", GH_ParamAccess.item);
        pManager.AddIntegerParameter("V2", "V2", "Value 2", GH_ParamAccess.item);

        pManager[0].Optional = true;
        pManager[1].Optional = true;
        pManager[2].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new ParamCustomDataType(), "Custom", "CNP", "New or manipulated object", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        CustomDataType? customData = null;
        int a = 0, b = 0;
        var hasObject = DA.GetData(0, ref customData);
        var has1 = DA.GetData(1, ref a);
        var has2 = DA.GetData(2, ref b);

        if (!hasObject || customData is null)
            customData = new();
        else
            customData = customData.Clone();

        if (has1)
            customData.TestData1 = a;
        if (has2)
            customData.TestData2 = b;

        DA.SetData(0, customData);
    }
}
