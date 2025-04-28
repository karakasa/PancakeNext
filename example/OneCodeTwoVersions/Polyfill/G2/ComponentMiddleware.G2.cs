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
public abstract partial class ComponentMiddleware<TComp> : GH_Component where TComp : ComponentMiddleware<TComp>
{
    protected ComponentMiddleware(string name, string nickname, string desc, string category, string subcategory) :
        base(CreateNomen(name, nickname, desc, category, subcategory))
    {
    }

    protected ComponentMiddleware(IReader reader) : base(reader) { }

    private static Nomen CreateNomen(string name, string nickname, string desc, string category, string subcategory)
    {
        var slot = ComponentSlotCacher.Instance[typeof(TComp)].ToSlot();
        var obsolete = ObsoleteCacher.Instance[typeof(TComp)];
        return new Nomen(name, desc, category, subcategory, slot: slot.Slot, rank: obsolete ? Rank.Hidden : slot.Rank, null);
    }
}

#endif