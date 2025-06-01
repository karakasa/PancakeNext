#if G2
using Grasshopper2.Components;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Grasshopper2.UI;
using GrasshopperIO;

namespace OneCodeTwoVersions.Polyfill;
public abstract partial class ComponentMiddleware<TSelf> : GH_Component where TSelf : ComponentMiddleware<TSelf>
{
    protected ComponentMiddleware(string name, string nickname, string desc, string category, string subcategory) :
        base(CreateNomen(name, nickname, desc, category, subcategory))
    {
    }

    protected ComponentMiddleware(IReader reader) : base(reader) { }

    private static Nomen CreateNomen(string name, string nickname, string desc, string category, string subcategory)
    {
        var slot = ComponentSlotCacher.Instance[typeof(TSelf)].ToSlot();
        var obsolete = ObsoleteCacher.Instance[typeof(TSelf)];
        return new Nomen(name, desc, category, subcategory, slot: slot.Slot, rank: obsolete ? Rank.Hidden : slot.Rank, null);
    }
}

#endif