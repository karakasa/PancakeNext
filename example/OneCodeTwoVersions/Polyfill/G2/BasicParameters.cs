using Grasshopper2.Extensions;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.Types.Colour;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public sealed class Param_Boolean : GH_PersistentParam<GH_Boolean, BooleanParameter, bool> { }
public sealed class Param_Point : GH_PersistentParam<GH_Point, Point3Parameter, Point3d> { }
public sealed class Param_Vector : GH_PersistentParam<GH_Vector, VectorParameter, Vector3d> { }
public sealed class Param_Transform : GH_PersistentParam<GH_Transform, TransformParameter, Transform> { }
public sealed class Param_Plane : GH_PersistentParam<GH_Plane, PlaneParameter, Plane> { }
public sealed class Param_Box : GH_PersistentParam<GH_Box, BoxParameter, Box> { }
public sealed class Param_Line : GH_PersistentParam<GH_Line, LineParameter, Line> { }
public sealed class Param_Circle : GH_PersistentParam<GH_Circle, CircleParameter, Circle> { }
public sealed class Param_Rectangle : GH_PersistentParam<GH_Rectangle, RectangleParameter, Rectangle3d> { }
public sealed class Param_Arc : GH_PersistentParam<GH_Arc, ArcParameter, Arc> { }
public sealed class Param_Mesh : GH_PersistentParam<GH_Mesh, MeshParameter, Mesh> { }
public sealed class Param_MeshFace : GH_PersistentParam<GH_MeshFace, MeshFacetParameter, MeshFace> { }
public sealed class Param_Integer : GH_PersistentParam<GH_Integer, IntegerParameter, int> { }
public sealed class Param_Number : GH_PersistentParam<GH_Number, NumberParameter, double> { }
public sealed class Param_Time : GH_PersistentParam<GH_Time, DateTimeParameter, DateTime> { }
public sealed class Param_Text : GH_PersistentParam<GH_Text, TextParameter, string> { }
public sealed class Param_Interval : GH_PersistentParam<GH_Interval, IntervalParameter, Interval> { }
public sealed class Param_Colour : GH_PersistentParam<GH_Colour, ColourParameter, System.Drawing.Color, Grasshopper2.Types.Colour.Colour>
{
    protected override Color To1(Colour goo) => goo.ToEto().ToGdi();

    protected override Colour To2(Color goo) => Colour.FromGdi(goo);
}
public sealed class Param_StructurePath : GH_PersistentParam<GH_StructurePath, PathParameter, GH_Path, Grasshopper2.Data.Path>
{
    protected override GH_Path To1(Grasshopper2.Data.Path goo) => goo.To1();

    protected override Grasshopper2.Data.Path To2(GH_Path goo) => goo.To2();
}
public sealed class Param_Curve : GH_Param<GH_Curve, CurveParameter, Curve> { }
public sealed class Param_Surface : GH_Param<GH_Surface, SurfaceParameter, Surface> { }
public sealed class Param_Brep : GH_Param<GH_Brep, SurfaceParameter, Brep> { }
public sealed class Param_SubD : GH_Param<GH_SubD, SurfaceParameter, SubD> { }
public sealed class Param_Geometry : GH_Param<GH_Geometry, GenericParameter, GeometryBase> { }
public sealed class Param_ObjectWrapper : GH_Param<GH_ObjectWrapper, GenericParameter, object> { }
