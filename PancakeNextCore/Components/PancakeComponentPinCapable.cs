using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Components;
public abstract class PancakeComponentPinCapable<T> : PancakeComponent<T>, IPinCushion
    where T : IPancakeLocalizable<T>
{
    protected PancakeComponentPinCapable() : base()
    {
    }
    protected PancakeComponentPinCapable(IReader reader) : base(reader)
    {
    }

    public abstract IEnumerable<Guid> SupportedPins { get; }
}
