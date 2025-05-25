using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
public interface ICustomComparer
{
    int Count { get; }
    CustomComparer GetAt(int index);
}
