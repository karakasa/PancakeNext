using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using GrasshopperIO;
using OneCodeTwoVersions.Polyfill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions;
[IoId("{84CC03EC-2C32-4B98-B493-95AD6CD7AB97}")]
public sealed class ParamCustomDataType : ParamMiddleware<ParamCustomDataType, CustomDataTypeGoo, CustomDataType>
{
    public ParamCustomDataType() : base("DemoParam", "DemoParam", "Demo custom type data holder for Grasshopper 1 and 2", "Demo", "Demo")
    {
    }
    public ParamCustomDataType(IReader reader) : base(reader)
    {
    }
    protected override CustomDataTypeGoo InstantiateT() => new();
}
