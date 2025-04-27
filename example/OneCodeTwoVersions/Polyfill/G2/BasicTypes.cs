using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plane = Rhino.Geometry.Plane;

namespace OneCodeTwoVersions.Polyfill;
public sealed class GH_Boolean() : GH_Goo<bool>("Boolean")
{
    public GH_Boolean(GH_Boolean other) : this() { Value = other.Value; }
    public GH_Boolean(bool value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Boolean(this);
}

public sealed class GH_Point : GH_Goo<Point3d>
{
    public GH_Point() : base("Point") { }
    public GH_Point(GH_Point other) : this() { Value = other.Value; }
    public GH_Point(Point3d value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Point(this);
}

public sealed class GH_Vector : GH_Goo<Vector3d>
{
    public GH_Vector() : base("Vector") { }
    public GH_Vector(GH_Vector other) : this() { Value = other.Value; }
    public GH_Vector(Vector3d value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Vector(this);
}

public sealed class GH_Transform : GH_Goo<Transform>
{
    public GH_Transform() : base("Transform") { }
    public GH_Transform(GH_Transform other) : this() { Value = other.Value; }
    public GH_Transform(Transform value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Transform(this);
}

public sealed class GH_Plane : GH_Goo<Plane>
{
    public GH_Plane() : base("Plane") { }
    public GH_Plane(GH_Plane other) : this() { Value = other.Value; }
    public GH_Plane(Plane value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Plane(this);
}

public sealed class GH_Box : GH_Goo<Box>
{
    public GH_Box() : base("Box") { }
    public GH_Box(GH_Box other) : this() { Value = other.Value; }
    public GH_Box(Box value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Box(this);
}

public sealed class GH_Line : GH_Goo<Line>
{
    public GH_Line() : base("Line") { }
    public GH_Line(GH_Line other) : this() { Value = other.Value; }
    public GH_Line(Line value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Line(this);
}

public sealed class GH_Circle : GH_Goo<Circle>
{
    public GH_Circle() : base("Circle") { }
    public GH_Circle(GH_Circle other) : this() { Value = other.Value; }
    public GH_Circle(Circle value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Circle(this);
}

public sealed class GH_Rectangle : GH_Goo<Rectangle3d>
{
    public GH_Rectangle() : base("Rectangle") { }
    public GH_Rectangle(GH_Rectangle other) : this() { Value = other.Value; }
    public GH_Rectangle(Rectangle3d value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Rectangle(this);
}

public sealed class GH_Arc : GH_Goo<Arc>
{
    public GH_Arc() : base("Arc") { }
    public GH_Arc(GH_Arc other) : this() { Value = other.Value; }
    public GH_Arc(Arc value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Arc(this);
}

public sealed class GH_Curve : GH_Goo<Curve>
{
    public GH_Curve() : base("Curve") { }
    public GH_Curve(GH_Curve other) : this() { Value = other.Value; }
    public GH_Curve(Curve value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Curve(this);
}

public sealed class GH_Surface : GH_Goo<Surface>
{
    public GH_Surface() : base("Surface") { }
    public GH_Surface(GH_Surface other) : this() { Value = other.Value; }
    public GH_Surface(Surface value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Surface(this);
}

public sealed class GH_Brep : GH_Goo<Brep>
{
    public GH_Brep() : base("Brep") { }
    public GH_Brep(GH_Brep other) : this() { Value = other.Value; }
    public GH_Brep(Brep value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Brep(this);
}

public sealed class GH_SubD : GH_Goo<SubD>
{
    public GH_SubD() : base("SubD") { }
    public GH_SubD(GH_SubD other) : this() { Value = other.Value; }
    public GH_SubD(SubD value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_SubD(this);
}

public sealed class GH_Mesh : GH_Goo<Mesh>
{
    public GH_Mesh() : base("Mesh") { }
    public GH_Mesh(GH_Mesh other) : this() { Value = other.Value; }
    public GH_Mesh(Mesh value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Mesh(this);
}

public sealed class GH_MeshFace : GH_Goo<MeshFace>
{
    public GH_MeshFace() : base("MeshFace") { }
    public GH_MeshFace(GH_MeshFace other) : this() { Value = other.Value; }
    public GH_MeshFace(MeshFace value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_MeshFace(this);
}

public sealed class GH_Geometry : GH_Goo<GeometryBase>
{
    public GH_Geometry() : base("Geometry") { }
    public GH_Geometry(GH_Geometry other) : this() { Value = other.Value; }
    public GH_Geometry(GeometryBase value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Geometry(this);
}

public sealed class GH_Integer : GH_Goo<int>
{
    public GH_Integer() : base("Integer") { }
    public GH_Integer(GH_Integer other) : this() { Value = other.Value; }
    public GH_Integer(int value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Integer(this);
}

public sealed class GH_Number : GH_Goo<double>
{
    public GH_Number() : base("Number") { }
    public GH_Number(GH_Number other) : this() { Value = other.Value; }
    public GH_Number(double value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Number(this);
}

public sealed class GH_Time : GH_Goo<DateTime>
{
    public GH_Time() : base("Time") { }
    public GH_Time(GH_Time other) : this() { Value = other.Value; }
    public GH_Time(DateTime value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Time(this);
}

public sealed class GH_Colour : GH_Goo<Color>
{
    public GH_Colour() : base("Colour") { }
    public GH_Colour(GH_Colour other) : this() { Value = other.Value; }
    public GH_Colour(Color value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Colour(this);
}

public sealed class GH_Text : GH_Goo<string>
{
    public GH_Text() : base("Text") { }
    public GH_Text(GH_Text other) : this() { Value = other.Value; }
    public GH_Text(string value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Text(this);
}

public sealed class GH_Interval : GH_Goo<Interval>
{
    public GH_Interval() : base("Interval") { }
    public GH_Interval(GH_Interval other) : this() { Value = other.Value; }
    public GH_Interval(Interval value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_Interval(this);
}

public sealed class GH_StructurePath : GH_Goo<GH_Path>
{
    public GH_StructurePath() : base("Path") { }
    public GH_StructurePath(GH_StructurePath other) : this() { Value = other.Value; }
    public GH_StructurePath(GH_Path value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_StructurePath(this);
}

public sealed class GH_ObjectWrapper : GH_Goo<object>
{
    public GH_ObjectWrapper() : base("Goo") { }
    public GH_ObjectWrapper(GH_ObjectWrapper other) : this() { Value = other.Value; }
    public GH_ObjectWrapper(object value) : this() { Value = value; }
    public override IGH_Goo Duplicate() => new GH_ObjectWrapper(this);
}