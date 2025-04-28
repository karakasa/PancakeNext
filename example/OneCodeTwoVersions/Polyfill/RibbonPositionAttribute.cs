using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if G2
using Grasshopper2.UI;
#else
using Grasshopper.Kernel;
#endif

namespace OneCodeTwoVersions.Polyfill;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class RibbonPositionAttribute : Attribute
{
    private int _slot { get; }
    public bool Hidden { get; set; }
    public bool Obsecure { get; set; }

    public RibbonPositionAttribute(int slot)
    {
        _slot = slot;
    }
#if G2
    internal (int Slot, Rank Rank) ToSlot()
    {
        if (Hidden) return (_slot, Rank.Hidden);
        if (Obsecure) return (_slot, Rank.Obscure);

        return (_slot, Rank.Normal);
    }
#else
    internal GH_Exposure ToExposure()
    {
        if (Hidden) return GH_Exposure.hidden;

        var flag = _slot switch
        {
            0 => GH_Exposure.primary,
            1 => GH_Exposure.secondary,
            2 => GH_Exposure.tertiary,
            3 => GH_Exposure.quarternary, // typo from David
            4 => GH_Exposure.quinary,
            5 => GH_Exposure.senary,
            6 => GH_Exposure.septenary,
            7 => GH_Exposure.octonary,
            _ => GH_Exposure.last,
        };

        if (Obsecure) flag |= GH_Exposure.obscure;
        return flag;
    }
#endif
}
