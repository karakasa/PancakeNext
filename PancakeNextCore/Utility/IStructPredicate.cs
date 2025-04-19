using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal interface IStructPredicate<T>
{
    bool Predicate(T val);
}
