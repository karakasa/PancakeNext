using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompatibleLayer;
public enum GH_RuntimeMessageLevel
{
    Blank = 0,
    Remark = byte.MaxValue,
    Warning = 10,
    Error = 20
}
