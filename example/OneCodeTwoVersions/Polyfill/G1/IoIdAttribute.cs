using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrasshopperIO;

#if G1

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IoIdAttribute(string id) : Attribute
{
    public Guid Id { get; } = Guid.Parse(id);
}

#endif