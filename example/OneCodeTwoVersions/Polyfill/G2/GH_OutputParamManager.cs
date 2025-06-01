#if G2
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Types.Numeric;
using Rhino.Geometry;

namespace OneCodeTwoVersions.Polyfill;

public sealed class GH_OutputParamManager(Component comp, OutputAdder adder) : GH_ParamManager
{
    private readonly Component _owner = comp;
    private readonly OutputAdder _adder = adder;

    public override int ParamCount => _adder.Count;

    public override IGH_Param this[int index] => ParameterWrapper.CreateFrom(_owner.Parameters.Output(index));

    public int AddParameter(IParameter param)
    {
        _adder.Add(param);
        return _adder.Count - 1;
    }

    public int AddParameter(IParameter p, string name, string nickname, string description, GH_ParamAccess access)
    {
        var param = ParameterWrapper.CreateFrom(p);

        param.Name = name;
        param.NickName = nickname;
        param.Description = description;
        param.Access = access;
        return AddParameter(param);
    }

    public int AddParameter(IGH_Param param)
    {
        _adder.Add(param.UnderlyingObject);
        return _adder.Count - 1;
    }

    public int AddParameter(IGH_Param param, string name, string nickname, string description, GH_ParamAccess access)
    {
        param.Name = name;
        param.NickName = nickname;
        param.Description = description;
        param.Access = access;
        return AddParameter(param);
    }

    public int AddPointParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddPoint(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddVectorParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddVector(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }
    public int AddFieldParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddField(name, nickname, description, access.To2());
        return _adder.Count - 1;
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

    public int AddBoxParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddBox(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }
    public int AddLineParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddLine(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddCircleParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddCircle(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddRectangleParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddRectangle(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }


    public int AddArcParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddArc(name, nickname, description, access.To2());
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

    public int AddIntegerParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddInteger(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddNumberParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddNumber(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddAngleParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddAngle(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddComplexNumberParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddComplex(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddTimeParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddDateTime(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddColourParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddColour(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddTextParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddText(name, nickname, description, access.To2());
        return _adder.Count - 1;
    }

    public int AddIntervalParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        _adder.AddInterval(name, nickname, description, access.To2());
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
