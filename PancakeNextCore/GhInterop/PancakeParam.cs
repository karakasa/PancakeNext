using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GhInterop;

public abstract class PancakeParam<T> : Parameter<T>
{
    protected PancakeParam(IReader reader) : base(reader)
    {
    }

    protected PancakeParam(Nomen nomen, Access access = Access.Tree) : base(nomen, access)
    {
    }

    protected PancakeParam(string name, string userName, string info, Access access = Access.Tree) : base(name, userName, info, access)
    {
    }
}
