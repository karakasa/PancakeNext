using Grasshopper2.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper2.Parameters.Standard;
using Eto.Drawing;
using Grasshopper2.Types.Numeric;

#if G2
namespace OneCodeTwoVersions.Polyfill;
public sealed class GH_InputParamManager(Component comp, InputAdder adder) : GH_ParamManager
{
    private readonly Component _owner = comp;
    private readonly InputAdder _adder = adder;

    public override int ParamCount => _adder.Count;

    public override IGH_Param this[int index] => new(_owner.Parameters.Input(index));

    public int AddParameter(IGH_Param param)
    {
        throw new NotSupportedException();
    }

    public int AddParameter(IGH_Param param, string name, string nickname, string description, GH_ParamAccess access)
    {
        throw new NotSupportedException();
    }

    public int AddPointParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddPoint(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddPointParameter(string name, string nickname, string description, GH_ParamAccess access, Point3d @default)
    {
        _adder.AddPoint(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddPointParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<Point3d> @default)
    {
        _adder.AddPoint(name, nickname, description, access.To2()).Set([..@default]);
        return _adder.Count - 1;
    }

    public int AddVectorParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddVector(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddVectorParameter(string name, string nickname, string description, GH_ParamAccess access, Vector3d @default)
    {
        _adder.AddVector(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddFieldParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddField(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddMatrixParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        throw new NotSupportedException();
    }

    public int AddTransformParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddTransform(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddPlaneParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddPlane(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddPlaneParameter(string name, string nickname, string description, GH_ParamAccess access, Plane @default)
    {
        _adder.AddPlane(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddBoxParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddBox(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddBoxParameter(string name, string nickname, string description, GH_ParamAccess access, Box @default)
    {
        _adder.AddBox(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddLineParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddLine(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddLineParameter(string name, string nickname, string description, GH_ParamAccess access, Line @default)
    {
        _adder.AddLine(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddCircleParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddCircle(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddCircleParameter(string name, string nickname, string description, GH_ParamAccess access, Circle @default)
    {
        _adder.AddCircle(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddRectangleParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddRectangle(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddRectangleParameter(string name, string nickname, string description, GH_ParamAccess access, Rectangle3d @default)
    {
        _adder.AddRectangle(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddArcParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddArc(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddArcParameter(string name, string nickname, string description, GH_ParamAccess access, Arc @default)
    {
        _adder.AddArc(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddCurveParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddCurve(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddSurfaceParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddSurface(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddBrepParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddSurface(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddSubDParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddSurface(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddMeshParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddMesh(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddMeshFaceParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddMeshFacet(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddGeometryParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddGeneric(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddBooleanParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddBoolean(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddBooleanParameter(string name, string nickname, string description, GH_ParamAccess access, bool @default)
    {
        _adder.AddBoolean(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddBooleanParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<bool> @default)
    {
        _adder.AddBoolean(name, nickname, description, access.To2()).Set([..@default]);
        return _adder.Count - 1;
    }

    public int AddIntegerParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddInteger(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddIntegerParameter(string name, string nickname, string description, GH_ParamAccess access, int @default)
    {
        _adder.AddInteger(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddIntegerParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<int> @default)
    {
        _adder.AddInteger(name, nickname, description, access.To2()).Set([..@default]);
        return _adder.Count - 1;
    }

    public int AddNumberParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddNumber(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddNumberParameter(string name, string nickname, string description, GH_ParamAccess access, double @default)
    {
        _adder.AddNumber(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddNumberParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<double> @default)
    {
        _adder.AddNumber(name, nickname, description, access.To2()).Set([..@default]);
        return _adder.Count - 1;
    }

    public int AddAngleParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddAngle(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddAngleParameter(string name, string nickname, string description, GH_ParamAccess access, double @default)
    {
        _adder.AddAngle(name, nickname, description, access.To2()).Set(new Angle(@default, AngleKind.Radian));
        return _adder.Count - 1;
    }

    public int AddAngleParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<double> @default)
    {
        _adder.AddAngle(name, nickname, description, access.To2()).Set([.. @default.Select(x => new Angle(x, AngleKind.Radian))]);
        return _adder.Count - 1;
    }

    public int AddComplexNumberParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddComplex(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    // GH_ComplexNumber is not polyfilled
    //public int AddComplexNumberParameter(string name, string nickname, string description, GH_ParamAccess access, GH_ComplexNumber @default)
    //{
    //    _adder.AddComplex(name, nickname, description, access.To2()).Set(@default);
    //    return _adder.Count - 1;
    //}

    public int AddTimeParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddDateTime(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddTimeParameter(string name, string nickname, string description, GH_ParamAccess access, DateTime @default)
    {
        _adder.AddDateTime(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddColourParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddColour(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddColourParameter(string name, string nickname, string description, GH_ParamAccess access, System.Drawing.Color @default)
    {
        _adder.AddColour(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddColourParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<System.Drawing.Color> @default)
    {
        _adder.AddColour(name, nickname, description, access.To2()).Set([..@default]);
        return _adder.Count - 1;
    }

    public int AddTextParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddText(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddTextParameter(string name, string nickname, string description, GH_ParamAccess access, string @default)
    {
        _adder.AddText(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddTextParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<string> @default)
    {
        _adder.AddText(name, nickname, description, access.To2()).Set([..@default]);
        return _adder.Count - 1;
    }

    public int AddIntervalParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddInterval(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddIntervalParameter(string name, string nickname, string description, GH_ParamAccess access, Interval @default)
    {
        _adder.AddInterval(name, nickname, description, access.To2()).Set(@default);
        return _adder.Count - 1;
    }

    public int AddIntervalParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<Interval> @default)
    {
        _adder.AddInterval(name, nickname, description, access.To2()).Set([..@default]);
        return _adder.Count - 1;
    }

    public int AddPathParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddPath(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddGenericParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddGeneric(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }
}
#endif