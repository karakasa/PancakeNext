#if G1
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract class ComponentMiddleware<TSelf> : GH_Component where TSelf : ComponentMiddleware<TSelf>
{
    protected ComponentMiddleware(string name, string nickname, string desc, string category, string subcategory) : base(name, nickname, desc, category, subcategory)
    {
    }

    protected ComponentMiddleware(IReader reader)
    {
        // Placeholder
    }
    public override Guid ComponentGuid => ComponentIdCacher.Instance[GetType()];
    public override GH_Exposure Exposure => ComponentSlotCacher.Instance[GetType()].ToExposure();
}

#endif