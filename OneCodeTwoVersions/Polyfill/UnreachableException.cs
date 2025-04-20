using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if !NET

namespace System.Diagnostics;
public sealed class UnreachableException : Exception
{
}

#endif