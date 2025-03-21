using Eto.Drawing;
using Grasshopper2.UI.Animation;
using Grasshopper2.UI.Icon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Utility;
public sealed class SvgGhIcon(Icon icon) : AbstractIcon(IconType.Vector)
{
    private readonly Icon _icon = icon;

    protected override void DrawInternal(IconContext context)
    {
    }

    public static IIcon CreateFromSvgResource(string uri, Assembly assembly, int defaultWidth, int defaultHeight)
    {
        var icon = Rhino.UI.ImageResources.CreateEtoIcon(uri, assembly, defaultWidth, defaultHeight, false);
        return new SvgGhIcon(icon);
    }

    public static IIcon CreateFromSvgResource(string name, int defaultWidth, int defaultHeight)
    {
        return CreateFromSvgResource($"PancakeNextCore.Icons.{name}.svg", typeof(SvgGhIcon).Assembly, defaultWidth, defaultHeight);
    }
}