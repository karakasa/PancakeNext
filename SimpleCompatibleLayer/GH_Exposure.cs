using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompatibleLayer;
[Flags]
public enum GH_Exposure
{
    hidden = -1,
    primary = 2,
    secondary = 4,
    tertiary = 8,
    quarternary = 0x10,
    quinary = 0x20,
    senary = 0x40,
    septenary = 0x80,
    obscure = 0x10000
}
