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
    protected abstract Guid[] GetSupportedPins();
    private Guid[]? _supportedPins;
    public IEnumerable<Guid> SupportedPins => _supportedPins ??= GetSupportedPins();
}
