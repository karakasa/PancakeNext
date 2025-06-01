using Grasshopper2.Extensions;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.Types.Colour;
using Rhino.Geometry;
using Rhino.Render;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public sealed class Param_Boolean : GH_PersistentParam<GH_Boolean, BooleanParameter, bool>
{
    public Param_Boolean() { }
    public Param_Boolean(BooleanParameter p) : base(p) { }
}
public sealed class Param_Point : GH_PersistentParam<GH_Point, Point3Parameter, Point3d>
{
    public Param_Point() { }
    public Param_Point(Point3Parameter p) : base(p) { }
}
public sealed class Param_Vector : GH_PersistentParam<GH_Vector, VectorParameter, Vector3d>
{
    public Param_Vector() { }
    public Param_Vector(VectorParameter p) : base(p) { }
}
public sealed class Param_Transform : GH_PersistentParam<GH_Transform, TransformParameter, Transform>
{
    public Param_Transform() { }
    public Param_Transform(TransformParameter p) : base(p) { }
}
public sealed class Param_Plane : GH_PersistentParam<GH_Plane, PlaneParameter, Plane>
{
    public Param_Plane() { }
    public Param_Plane(PlaneParameter p) : base(p) { }
}
public sealed class Param_Box : GH_PersistentParam<GH_Box, BoxParameter, Box>
{
    public Param_Box() { }
    public Param_Box(BoxParameter p) : base(p) { }
}
public sealed class Param_Line : GH_PersistentParam<GH_Line, LineParameter, Line>
{
    public Param_Line() { }
    public Param_Line(LineParameter p) : base(p) { }
}
public sealed class Param_Circle : GH_PersistentParam<GH_Circle, CircleParameter, Circle>
{
    public Param_Circle() { }
    public Param_Circle(CircleParameter p) : base(p) { }
}
public sealed class Param_Rectangle : GH_PersistentParam<GH_Rectangle, RectangleParameter, Rectangle3d>
{
    public Param_Rectangle() { }
    public Param_Rectangle(RectangleParameter p) : base(p) { }
}
public sealed class Param_Arc : GH_PersistentParam<GH_Arc, ArcParameter, Arc>
{
    public Param_Arc() { }
    public Param_Arc(ArcParameter p) : base(p) { }
}
public sealed class Param_Mesh : GH_PersistentParam<GH_Mesh, MeshParameter, Mesh>
{
    public Param_Mesh() { }
    public Param_Mesh(MeshParameter p) : base(p) { }
}
public sealed class Param_MeshFace : GH_PersistentParam<GH_MeshFace, MeshFacetParameter, MeshFace>
{
    public Param_MeshFace() { }
    public Param_MeshFace(MeshFacetParameter p) : base(p) { }
}
public sealed class Param_Integer : GH_PersistentParam<GH_Integer, IntegerParameter, int>
{
    public Param_Integer() { }
    public Param_Integer(IntegerParameter p) : base(p) { }
    public void AddNamedValue(string name, int value)
    {
        var presets = Value.Presets;
        if (!presets.IsAvailable(name))
            throw new NotSupportedException("GH2 doesn't allow replacing same-named preset.");
        else
            presets.Add(name, name, value);
    }
    public void ClearNamedValues() => Value.Presets?.Clear();
    public bool HasNamedValues => Value.Presets?.Count > 0;

}
public sealed class Param_Number : GH_PersistentParam<GH_Number, NumberParameter, double>
{
    public Param_Number() { }
    public Param_Number(NumberParameter p) : base(p) { }
}
public sealed class Param_Time : GH_PersistentParam<GH_Time, DateTimeParameter, DateTime>
{
    public Param_Time() { }
    public Param_Time(DateTimeParameter p) : base(p) { }
}
public sealed class Param_Text : GH_PersistentParam<GH_Text, TextParameter, string>
{
    public Param_Text() { }
    public Param_Text(TextParameter p) : base(p) { }
}
public sealed class Param_Interval : GH_PersistentParam<GH_Interval, IntervalParameter, Interval>
{
    public Param_Interval() { }
    public Param_Interval(IntervalParameter p) : base(p) { }
}
public sealed class Param_Colour : GH_PersistentParam<GH_Colour, ColourParameter, System.Drawing.Color, Grasshopper2.Types.Colour.Colour>
{
    public Param_Colour() { }
    public Param_Colour(ColourParameter p) : base(p) { }

    protected override Color To1(Colour goo) => goo.ToEto().ToGdi();
    protected override Colour To2(Color goo) => Colour.FromGdi(goo);
}
public sealed class Param_StructurePath : GH_PersistentParam<GH_StructurePath, PathParameter, GH_Path, Grasshopper2.Data.Path>
{
    public Param_StructurePath() { }
    public Param_StructurePath(PathParameter p) : base(p) { }

    protected override GH_Path To1(Grasshopper2.Data.Path goo) => goo.To1();
    protected override Grasshopper2.Data.Path To2(GH_Path goo) => goo.To2();
}
public sealed class Param_Curve : GH_Param<GH_Curve, CurveParameter, Curve>
{
    public Param_Curve() { }
    public Param_Curve(CurveParameter p) : base(p) { }
}
public sealed class Param_Surface : GH_Param<GH_Surface, SurfaceParameter, Surface>
{
    public Param_Surface() { }
    public Param_Surface(SurfaceParameter p) : base(p) { }
}
public sealed class Param_Brep : GH_Param<GH_Brep, SurfaceParameter, Brep>
{
    public Param_Brep() { }
    public Param_Brep(SurfaceParameter p) : base(p) { }
}
public sealed class Param_SubD : GH_Param<GH_SubD, SurfaceParameter, SubD>
{
    public Param_SubD() { }
    public Param_SubD(SurfaceParameter p) : base(p) { }
}
public sealed class Param_Geometry : GH_Param<GH_Geometry, GenericParameter, GeometryBase>
{
    public Param_Geometry() { }
    public Param_Geometry(GenericParameter p) : base(p) { }
}
public sealed class Param_ObjectWrapper : GH_Param<GH_ObjectWrapper, GenericParameter, object>
{
    public Param_ObjectWrapper() { }
    public Param_ObjectWrapper(GenericParameter p) : base(p) { }
}
