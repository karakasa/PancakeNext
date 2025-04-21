using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal interface IStructFunc<T1, TReturn>
{
    public TReturn Invoke(T1 param);
}

internal interface IStructFunc<T1, T2, TReturn>
{
    public TReturn Invoke(T1 p1, T2 p2);
}