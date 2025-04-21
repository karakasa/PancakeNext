using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Interfaces;
internal interface IPancakeLocalizableStatic<T> where T : IPancakeLocalizableStatic<T>
{
#if NET
    static string StaticLocalizedName { get; }
    static string StaticLocalizedDescription { get; }
#endif
}
