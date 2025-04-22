using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
internal static class GhGui
{
    internal static void ZoomToObject(IDocumentObject docObj)
    {
        var canvas = Editor.Instance?.Canvas;
        if (canvas is null || docObj is null) return;

        var frame = docObj.Attributes.Bounds.AdjustSides(20);
        canvas.Navigate(frame, (0.01f, Math.Max(1, canvas.Projection.Zoom)));
    }
}
