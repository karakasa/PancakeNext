using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal interface IStringPartExecutor
{
    bool HandlePart(string str, int partId, int startIndex, int length);
}
