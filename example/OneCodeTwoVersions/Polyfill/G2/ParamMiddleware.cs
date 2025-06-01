using Grasshopper2.Data.Meta;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract class ParamMiddleware<TSelf, TGoo, TData> : Parameter<TData>
    where TSelf : ParamMiddleware<TSelf, TGoo, TData>
{
    protected ParamMiddleware(string name, string nickname, string desc, string category, string subcategory, GH_ParamAccess access = GH_ParamAccess.tree)
        : base(CreateNomen(name, nickname, desc, category, subcategory), access.To2())
    {
    }
    protected ParamMiddleware(IReader reader) : base(reader)
    {
    }
    private static Nomen CreateNomen(string name, string nickname, string desc, string category, string subcategory)
    {
        var slot = ComponentSlotCacher.Instance[typeof(TSelf)].ToSlot();
        var obsolete = ObsoleteCacher.Instance[typeof(TSelf)];
        return new Nomen(name, desc, category, subcategory, slot: slot.Slot, rank: obsolete ? Rank.Hidden : slot.Rank, null);
    }
    protected virtual TGoo InstantiateT() => throw new UnreachableException();
    public new virtual System.Drawing.Bitmap? Icon => null;
    protected override IIcon? IconInternal
    {
        get
        {
            var icon = Icon;
            if (icon is null) return null;
            return AbstractIcon.FromBitmap(icon.ToEto());
        }
    }
}
