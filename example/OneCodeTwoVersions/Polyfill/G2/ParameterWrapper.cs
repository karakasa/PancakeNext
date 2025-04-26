using Grasshopper2.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
internal sealed class ParameterWrapper(IParameter p) : IGH_Param
{
    private readonly IParameter _param = p;
}
