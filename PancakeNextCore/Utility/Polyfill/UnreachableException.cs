using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Diagnostics;

#if !NET
public sealed class UnreachableException : Exception
{
}

#endif