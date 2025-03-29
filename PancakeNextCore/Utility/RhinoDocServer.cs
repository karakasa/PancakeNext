using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal static class RhinoDocServer
{
    public static RhinoDoc ActiveDoc
    {
        get
        {
            return RhinoDoc.ActiveDoc;
        }
    }

    public static UnitSystem ModelUnitSystem
    {
        get
        {
            return ActiveDoc?.ModelUnitSystem ?? UnitSystem.None;
        }
    }
}
