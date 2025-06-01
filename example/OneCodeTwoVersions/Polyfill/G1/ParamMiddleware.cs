using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract class ParamMiddleware<TSelf, TGoo, TData> : GH_Param<TGoo>
    where TSelf : ParamMiddleware<TSelf, TGoo, TData>
    where TGoo : GH_Goo<TData>
{
    protected ParamMiddleware(string name, string nickname, string desc, string category, string subcategory) : base(new GH_InstanceDescription(name, nickname, desc, category, subcategory))
    {
    }

    protected ParamMiddleware(IReader reader) : this("", "", "", "", "")
    {
        // Placeholder
    }
    public override Guid ComponentGuid => ComponentIdCacher.Instance[GetType()];
    public override GH_Exposure Exposure => ComponentSlotCacher.Instance[GetType()].ToExposure();
    protected override TGoo InstantiateT() => (TGoo)Activator.CreateInstance(typeof(TGoo));
    protected override TGoo PreferredCast(object data)
    {
        if (data is TData actualData)
        {
            var p = InstantiateT();
            p.Value = actualData;
            return p;
        }

        return base.PreferredCast(data);
    }
}
