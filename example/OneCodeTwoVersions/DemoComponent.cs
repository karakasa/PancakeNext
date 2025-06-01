#if G1
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

#endif
using GrasshopperIO;
using OneCodeTwoVersions.Polyfill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions;
[IoId("BF647C43-8B2F-49C6-84D3-708BCAF2F125")]
[RibbonPosition(0)]
public sealed class DemoComponent : ComponentMiddleware<DemoComponent>
{
    public DemoComponent() : base("Comp", "Comp", "Demo component for Grasshopper 1 and 2", "Demo", "Demo")
    {
    }

    public DemoComponent(IReader reader) : base(reader)
    {
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddIntegerParameter("List", "L", "List of integers to be added", GH_ParamAccess.list);
        var lastIdx = pManager.AddIntegerParameter("Pivot", "P", "Pivot", GH_ParamAccess.item, 0);

        var ip = (Param_Integer)pManager[lastIdx];
        ip.AddNamedValue("Zero", 0);
        ip.AddNamedValue("One", 1);
        ip.AddNamedValue("Two", 2);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddIntegerParameter("Smaller list", "L<", "List of integers smaller than the pivot", GH_ParamAccess.list);
        pManager.AddIntegerParameter("Sum of smaller ints", "S<", "Sum of integers smaller than the pivot", GH_ParamAccess.item);
        pManager.AddIntegerParameter("Larger list", "L>", "List of integers larger than the pivot", GH_ParamAccess.list);
        pManager.AddIntegerParameter("Sum of larger ints", "S.", "Sum of integers larger than the pivot", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var list = new List<int>();
        DA.GetDataList(0, list);

        if (list.Count == 0)
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "List is empty.");

        var pivot = 0;
        DA.GetData(1, ref pivot);

        var listSmaller = new List<int>();
        var listLarger = new List<int>();

        var sumSmaller = 0;
        var sumLarger = 0;

        foreach (var it in list)
        {
            if (it < pivot)
            {
                listSmaller.Add(it);
                sumSmaller += it;
            }
            else
            {
                listLarger.Add(it);
                sumLarger += it;
            }
        }

        DA.SetDataList(0, listSmaller);
        DA.SetDataList(2, listSmaller);

        DA.SetData(1, sumSmaller);
        DA.SetData(3, sumLarger);
    }
}
