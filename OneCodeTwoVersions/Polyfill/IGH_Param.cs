using Grasshopper2.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
#if G2
public sealed class IGH_Param(IParameter inner)
{
    private readonly IParameter _inner = inner;
}

#endif