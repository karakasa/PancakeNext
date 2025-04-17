using Eto.Forms;
using Grasshopper2.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;

internal static class EtoExtensions
{
    public static Window GetRhinoWindowAsEto()
    {
        return Rhino.UI.RhinoEtoApp.MainWindow;
    }
    public static Window GetGrasshopperWindowAsEto()
    {
        return Editor.Instance ?? GetRhinoWindowAsEto();
    }
}
