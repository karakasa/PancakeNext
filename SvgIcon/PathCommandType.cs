using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvgIcon;
public enum PathCommandType
{
    EOF,
    Line,
    Polyline,
    Arc,
    Ellipse,
    Bezier,
    Curve,
    Rectangle,
    NestedPath
}
