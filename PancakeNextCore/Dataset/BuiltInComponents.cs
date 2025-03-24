using Grasshopper2.UI;
using PancakeNextCore.Components.Io;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Dataset;
internal static partial class ComponentLibrary
{
    private static void AddBuiltinComponentList()
    {
        AddCategory("Export", [
            (typeof(pcExportSTL), "Export STL", "Export meshes to STL files.", 0),
            ]);
    }
}
