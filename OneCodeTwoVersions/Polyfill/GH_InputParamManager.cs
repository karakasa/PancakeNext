using Grasshopper2.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if G2
namespace OneCodeTwoVersions.Polyfill;
public sealed class GH_InputParamManager(InputAdder adder) : GH_ParamManager
{
    /// <summary>
    /// Returns the number of input parameters already part of this component.
    /// </summary>
    public override int ParamCount => adder.Count;

    /// <summary>
    /// Gets the input parameter at the given index.
    /// </summary>
    public override IGH_Param this[int index] => m_owner.Params.Input[index];

    /// <summary>
    /// Hide a specific input parameter. If the parameter at the given index implements IGH_PreviewObject 
    /// then the Hidden flag will be set to True. Otherwise, nothing will happen.
    /// </summary>
    /// <param name="index">Index of parameter to hide.</param>
    public override void HideParameter(int index)
    {
        if (index >= 0 && index < m_owner.Params.Input.Count)
        {
            IGH_Param iGH_Param = this[index];
            if (iGH_Param is IGH_PreviewObject)
            {
                ((IGH_PreviewObject)iGH_Param).Hidden = true;
            }
        }
    }

    /// <summary>
    /// Generic parameter addition. If you cannot use one of the utility methods provided 
    /// by this class, you can register a customized parameter using this method.
    /// </summary>
    /// <param name="param">The parameter to add.</param>
    /// <returns>Index of the newly added parameter.</returns>
    public int AddParameter(IGH_Param param)
    {
        m_owner.Params.Input.Add(param);
        return checked(m_owner.Params.Input.Count - 1);
    }

    /// <summary>
    /// Generic parameter addition. If you cannot use one of the utility methods provided 
    /// by this class, you can register a customized parameter using this method.
    /// </summary>
    /// <param name="param">The parameter to add.</param>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <returns>Index of the newly added parameter.</returns>
    public int AddParameter(IGH_Param param, string name, string nickname, string description, GH_ParamAccess access)
    {
        FixUpParameter(param, name, nickname, description);
        param.Access = access;
        return AddParameter(param);
    }

    /// <summary>
    /// Add a 3D Point parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddPointParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Point3d[] @default = null;
        return AddPointParameter(name, nickname, description, access, @default);
    }

    /// <summary>
    /// Register a new 3D point param with a single default coordinate.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddPointParameter(string name, string nickname, string description, GH_ParamAccess access, Point3d @default)
    {
        return AddPointParameter(name, nickname, description, access, new Point3d[1] { @default });
    }

    /// <summary>
    /// Register a new 3D point param with multiple default coordinates.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A collection of default values to store in the parameter Persistent Data.</param>
    public int AddPointParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<Point3d> @default)
    {
        Param_Point param_Point = new Param_Point();
        FixUpParameter(param_Point, name, nickname, description);
        param_Point.Access = access;
        if (@default != null)
        {
            param_Point.SetPersistentData(@default.ToArray());
        }
        return AddParameter(param_Point);
    }

    /// <summary>
    /// Add a 3D Vector parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddVectorParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Vector param_Vector = new Param_Vector();
        FixUpParameter(param_Vector, name, nickname, description);
        param_Vector.Access = access;
        return AddParameter(param_Vector);
    }

    /// <summary>
    /// Add a 3D Vector parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddVectorParameter(string name, string nickname, string description, GH_ParamAccess access, Vector3d @default)
    {
        Param_Vector param_Vector = new Param_Vector();
        FixUpParameter(param_Vector, name, nickname, description);
        param_Vector.Access = access;
        param_Vector.SetPersistentData(new GH_Vector(@default));
        return AddParameter(param_Vector);
    }

    /// <summary>
    /// Add a Field parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddFieldParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Field param_Field = new Param_Field();
        FixUpParameter(param_Field, name, nickname, description);
        param_Field.Access = access;
        return AddParameter(param_Field);
    }

    /// <summary>
    /// Add a Matrix parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddMatrixParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Matrix param_Matrix = new Param_Matrix();
        FixUpParameter(param_Matrix, name, nickname, description);
        param_Matrix.Access = access;
        return AddParameter(param_Matrix);
    }

    /// <summary>
    /// Add a Transformation parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddTransformParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Transform param_Transform = new Param_Transform();
        FixUpParameter(param_Transform, name, nickname, description);
        param_Transform.Access = access;
        return AddParameter(param_Transform);
    }

    /// <summary>
    /// Add a 3D Plane parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddPlaneParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Plane param_Plane = new Param_Plane();
        FixUpParameter(param_Plane, name, nickname, description);
        param_Plane.Access = access;
        return AddParameter(param_Plane);
    }

    /// <summary>
    /// Add a 3D Plane parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddPlaneParameter(string name, string nickname, string description, GH_ParamAccess access, Plane @default)
    {
        Param_Plane param_Plane = new Param_Plane();
        FixUpParameter(param_Plane, name, nickname, description);
        param_Plane.Access = access;
        param_Plane.SetPersistentData(new GH_Plane(@default));
        return AddParameter(param_Plane);
    }

    /// <summary>
    /// Add a 3D Box parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddBoxParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Box param_Box = new Param_Box();
        FixUpParameter(param_Box, name, nickname, description);
        param_Box.Access = access;
        return AddParameter(param_Box);
    }

    /// <summary>
    /// Add a 3D Box parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddBoxParameter(string name, string nickname, string description, GH_ParamAccess access, Box @default)
    {
        Param_Box param_Box = new Param_Box();
        FixUpParameter(param_Box, name, nickname, description);
        param_Box.Access = access;
        param_Box.SetPersistentData(new GH_Box(@default));
        return AddParameter(param_Box);
    }

    /// <summary>
    /// Add a 3D Line parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddLineParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Line param_Line = new Param_Line();
        FixUpParameter(param_Line, name, nickname, description);
        param_Line.Access = access;
        return AddParameter(param_Line);
    }

    /// <summary>
    /// Add a 3D Line parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddLineParameter(string name, string nickname, string description, GH_ParamAccess access, Line @default)
    {
        Param_Line param_Line = new Param_Line();
        FixUpParameter(param_Line, name, nickname, description);
        param_Line.Access = access;
        param_Line.SetPersistentData(new GH_Line(@default));
        return AddParameter(param_Line);
    }

    /// <summary>
    /// Add a 3D Circle parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddCircleParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Circle param_Circle = new Param_Circle();
        FixUpParameter(param_Circle, name, nickname, description);
        param_Circle.Access = access;
        return AddParameter(param_Circle);
    }

    /// <summary>
    /// Add a 3D Circle parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddCircleParameter(string name, string nickname, string description, GH_ParamAccess access, Circle @default)
    {
        Param_Circle param_Circle = new Param_Circle();
        FixUpParameter(param_Circle, name, nickname, description);
        param_Circle.Access = access;
        param_Circle.SetPersistentData(new GH_Circle(@default));
        return AddParameter(param_Circle);
    }

    /// <summary>
    /// Add a 3D Rectangle parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddRectangleParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Rectangle param_Rectangle = new Param_Rectangle();
        FixUpParameter(param_Rectangle, name, nickname, description);
        param_Rectangle.Access = access;
        return AddParameter(param_Rectangle);
    }

    /// <summary>
    /// Add a 3D Rectangle parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddRectangleParameter(string name, string nickname, string description, GH_ParamAccess access, Rectangle3d @default)
    {
        Param_Rectangle param_Rectangle = new Param_Rectangle();
        FixUpParameter(param_Rectangle, name, nickname, description);
        param_Rectangle.Access = access;
        param_Rectangle.SetPersistentData(new GH_Rectangle(@default));
        return AddParameter(param_Rectangle);
    }

    /// <summary>
    /// Add a 3D Arc parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddArcParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Arc param_Arc = new Param_Arc();
        FixUpParameter(param_Arc, name, nickname, description);
        param_Arc.Access = access;
        return AddParameter(param_Arc);
    }

    /// <summary>
    /// Add a 3D Arc parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddArcParameter(string name, string nickname, string description, GH_ParamAccess access, Arc @default)
    {
        Param_Arc param_Arc = new Param_Arc();
        FixUpParameter(param_Arc, name, nickname, description);
        param_Arc.Access = access;
        param_Arc.SetPersistentData(new GH_Arc(@default));
        return AddParameter(param_Arc);
    }

    /// <summary>
    /// Add a 3D Curve parameter to the input list of this component. 
    /// Curve parameters can handle all curve types (Lines, Polylines, Circles, Arcs, NurbsCurves etc.)
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddCurveParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Curve param_Curve = new Param_Curve();
        FixUpParameter(param_Curve, name, nickname, description);
        param_Curve.Access = access;
        return AddParameter(param_Curve);
    }

    /// <summary>
    /// Add a 3D Surface parameter to the input list of this component. 
    /// Surface parameters can handle both trimmed and untrimmed single-faced Breps.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddSurfaceParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Surface param_Surface = new Param_Surface();
        FixUpParameter(param_Surface, name, nickname, description);
        param_Surface.Access = access;
        return AddParameter(param_Surface);
    }

    /// <summary>
    /// Add a 3D Brep parameter to the input list of this component. 
    /// Brep parameters can handle both trimmed and untrimmed single or multi-faced Breps.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddBrepParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Brep param_Brep = new Param_Brep();
        FixUpParameter(param_Brep, name, nickname, description);
        param_Brep.Access = access;
        return AddParameter(param_Brep);
    }

    /// <summary>
    /// Add a 3D Sub-D parameter to the input list of this component. 
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddSubDParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_SubD param_SubD = new Param_SubD();
        FixUpParameter(param_SubD, name, nickname, description);
        param_SubD.Access = access;
        return AddParameter(param_SubD);
    }

    /// <summary>
    /// Add a 3D Mesh parameter to the input list of this component. 
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddMeshParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Mesh param_Mesh = new Param_Mesh();
        FixUpParameter(param_Mesh, name, nickname, description);
        param_Mesh.Access = access;
        return AddParameter(param_Mesh);
    }

    /// <summary>
    /// Add a topological MeshFace parameter to the input list of this component. 
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddMeshFaceParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_MeshFace param_MeshFace = new Param_MeshFace();
        FixUpParameter(param_MeshFace, name, nickname, description);
        param_MeshFace.Access = access;
        return AddParameter(param_MeshFace);
    }

    /// <summary>
    /// Add a generic 3D Geometry parameter to the input list of this component.
    /// Geometry parameters can handle all types that represent actual shapes.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddGeometryParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Geometry param_Geometry = new Param_Geometry();
        FixUpParameter(param_Geometry, name, nickname, description);
        param_Geometry.Access = access;
        return AddParameter(param_Geometry);
    }

    /// <summary>
    /// Add a Transform Group parameter to the input list of this component.
    /// Groups are collections of geometry that are transformed as one, they are not the same as Rhino groups.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddGroupParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Group param_Group = new Param_Group();
        FixUpParameter(param_Group, name, nickname, description);
        param_Group.Access = access;
        return AddParameter(param_Group);
    }

    /// <summary>
    /// Add a Boolean parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddBooleanParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Boolean param_Boolean = new Param_Boolean();
        FixUpParameter(param_Boolean, name, nickname, description);
        param_Boolean.Access = access;
        return AddParameter(param_Boolean);
    }

    /// <summary>
    /// Add a Boolean parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddBooleanParameter(string name, string nickname, string description, GH_ParamAccess access, bool @default)
    {
        Param_Boolean param_Boolean = new Param_Boolean();
        FixUpParameter(param_Boolean, name, nickname, description);
        param_Boolean.Access = access;
        param_Boolean.SetPersistentData(new GH_Boolean(@default));
        return AddParameter(param_Boolean);
    }

    /// <summary>
    /// Add a Boolean parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A collection of default values to store in the parameter Persistent Data.</param>
    public int AddBooleanParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<bool> @default)
    {
        Param_Boolean param_Boolean = new Param_Boolean();
        FixUpParameter(param_Boolean, name, nickname, description);
        param_Boolean.Access = access;
        param_Boolean.SetPersistentData(@default.ToArray());
        return AddParameter(param_Boolean);
    }

    /// <summary>
    /// Add a Integer parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddIntegerParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Integer param_Integer = new Param_Integer();
        FixUpParameter(param_Integer, name, nickname, description);
        param_Integer.Access = access;
        return AddParameter(param_Integer);
    }

    /// <summary>
    /// Add a Integer parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddIntegerParameter(string name, string nickname, string description, GH_ParamAccess access, int @default)
    {
        Param_Integer param_Integer = new Param_Integer();
        FixUpParameter(param_Integer, name, nickname, description);
        param_Integer.Access = access;
        param_Integer.SetPersistentData(@default);
        return AddParameter(param_Integer);
    }

    /// <summary>
    /// Add a Integer parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A collection of default values to store in the parameter Persistent Data.</param>
    public int AddIntegerParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<int> @default)
    {
        Param_Integer param_Integer = new Param_Integer();
        FixUpParameter(param_Integer, name, nickname, description);
        param_Integer.Access = access;
        param_Integer.SetPersistentData(@default.ToArray());
        return AddParameter(param_Integer);
    }

    /// <summary>
    /// Add a floating point Number parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddNumberParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Number param_Number = new Param_Number();
        FixUpParameter(param_Number, name, nickname, description);
        param_Number.Access = access;
        return AddParameter(param_Number);
    }

    /// <summary>
    /// Add a floating point Number parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddNumberParameter(string name, string nickname, string description, GH_ParamAccess access, double @default)
    {
        Param_Number param_Number = new Param_Number();
        FixUpParameter(param_Number, name, nickname, description);
        param_Number.Access = access;
        param_Number.SetPersistentData(@default);
        return AddParameter(param_Number);
    }

    /// <summary>
    /// Add a floating point Number parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A collection of default values to store in the parameter Persistent Data.</param>
    public int AddNumberParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<double> @default)
    {
        Param_Number param_Number = new Param_Number();
        FixUpParameter(param_Number, name, nickname, description);
        param_Number.Access = access;
        param_Number.SetPersistentData(@default.ToArray());
        return AddParameter(param_Number);
    }

    /// <summary>
    /// Add a floating point Number parameter to the input list of this component. 
    /// The parameter will be aware that it is representing angles.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddAngleParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Number param_Number = new Param_Number();
        param_Number.AngleParameter = true;
        FixUpParameter(param_Number, name, nickname, description);
        param_Number.Access = access;
        return AddParameter(param_Number);
    }

    /// <summary>
    /// Add a floating point Number parameter to the input list of this component.
    /// The parameter will be aware that it is representing angles.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddAngleParameter(string name, string nickname, string description, GH_ParamAccess access, double @default)
    {
        Param_Number param_Number = new Param_Number();
        param_Number.AngleParameter = true;
        FixUpParameter(param_Number, name, nickname, description);
        param_Number.Access = access;
        param_Number.SetPersistentData(@default);
        return AddParameter(param_Number);
    }

    /// <summary>
    /// Add a floating point Number parameter to the input list of this component.
    /// The parameter will be aware that it is representing angles.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A collection of default values to store in the parameter Persistent Data.</param>
    public int AddAngleParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<double> @default)
    {
        Param_Number param_Number = new Param_Number();
        param_Number.AngleParameter = true;
        FixUpParameter(param_Number, name, nickname, description);
        param_Number.Access = access;
        param_Number.SetPersistentData(@default.ToArray());
        return AddParameter(param_Number);
    }

    /// <summary>
    /// Add a Complex Number parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddComplexNumberParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Complex param_Complex = new Param_Complex();
        FixUpParameter(param_Complex, name, nickname, description);
        param_Complex.Access = access;
        return AddParameter(param_Complex);
    }

    /// <summary>
    /// Add a Complex Number parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddComplexNumberParameter(string name, string nickname, string description, GH_ParamAccess access, GH_ComplexNumber @default)
    {
        Param_Complex param_Complex = new Param_Complex();
        FixUpParameter(param_Complex, name, nickname, description);
        param_Complex.Access = access;
        param_Complex.SetPersistentData(@default);
        return AddParameter(param_Complex);
    }

    /// <summary>
    /// Add a Time parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddTimeParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Time param_Time = new Param_Time();
        FixUpParameter(param_Time, name, nickname, description);
        param_Time.Access = access;
        return AddParameter(param_Time);
    }

    /// <summary>
    /// Add a Time parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddTimeParameter(string name, string nickname, string description, GH_ParamAccess access, DateTime @default)
    {
        Param_Time param_Time = new Param_Time();
        FixUpParameter(param_Time, name, nickname, description);
        param_Time.Access = access;
        param_Time.SetPersistentData(@default);
        return AddParameter(param_Time);
    }

    /// <summary>
    /// Add a Cultue parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddCultureParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Culture param_Culture = new Param_Culture();
        FixUpParameter(param_Culture, name, nickname, description);
        param_Culture.Access = access;
        return AddParameter(param_Culture);
    }

    /// <summary>
    /// Add a Culture parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddCultureParameter(string name, string nickname, string description, GH_ParamAccess access, CultureInfo @default)
    {
        Param_Culture param_Culture = new Param_Culture();
        FixUpParameter(param_Culture, name, nickname, description);
        param_Culture.Access = access;
        if (@default != null)
        {
            param_Culture.SetPersistentData(@default);
        }
        return AddParameter(param_Culture);
    }

    /// <summary>
    /// Add an ARGB Colour parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddColourParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Colour param_Colour = new Param_Colour();
        FixUpParameter(param_Colour, name, nickname, description);
        param_Colour.Access = access;
        return AddParameter(param_Colour);
    }

    /// <summary>
    /// Add an ARGB Colour parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddColourParameter(string name, string nickname, string description, GH_ParamAccess access, Color @default)
    {
        Param_Colour param_Colour = new Param_Colour();
        FixUpParameter(param_Colour, name, nickname, description);
        param_Colour.Access = access;
        param_Colour.SetPersistentData(@default);
        return AddParameter(param_Colour);
    }

    /// <summary>
    /// Add an ARGB Colour parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A collection of default values to store in the parameter Persistent Data.</param>
    public int AddColourParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<Color> @default)
    {
        Param_Colour param_Colour = new Param_Colour();
        FixUpParameter(param_Colour, name, nickname, description);
        param_Colour.Access = access;
        param_Colour.SetPersistentData(@default.ToArray());
        return AddParameter(param_Colour);
    }

    /// <summary>
    /// Add a Text parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddTextParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_String param_String = new Param_String();
        FixUpParameter(param_String, name, nickname, description);
        param_String.Access = access;
        return AddParameter(param_String);
    }

    /// <summary>
    /// Add a Text parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddTextParameter(string name, string nickname, string description, GH_ParamAccess access, string @default)
    {
        Param_String param_String = new Param_String();
        FixUpParameter(param_String, name, nickname, description);
        param_String.Access = access;
        param_String.SetPersistentData(@default);
        return AddParameter(param_String);
    }

    /// <summary>
    /// Add a Text parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A collection of default values to store in the parameter Persistent Data.</param>
    public int AddTextParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<string> @default)
    {
        Param_String param_String = new Param_String();
        FixUpParameter(param_String, name, nickname, description);
        param_String.Access = access;
        param_String.SetPersistentData(@default.ToArray());
        return AddParameter(param_String);
    }

    /// <summary>
    /// Add an Interval (i.e. numeric domain) parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddIntervalParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Interval param_Interval = new Param_Interval();
        FixUpParameter(param_Interval, name, nickname, description);
        param_Interval.Access = access;
        return AddParameter(param_Interval);
    }

    /// <summary>
    /// Add an Interval (i.e. numeric domain) parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A default value to store in the parameter Persistent Data.</param>
    public int AddIntervalParameter(string name, string nickname, string description, GH_ParamAccess access, Interval @default)
    {
        Param_Interval param_Interval = new Param_Interval();
        FixUpParameter(param_Interval, name, nickname, description);
        param_Interval.Access = access;
        param_Interval.SetPersistentData(new GH_Interval(@default));
        return AddParameter(param_Interval);
    }

    /// <summary>
    /// Add an Interval (i.e. numeric domain) parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    /// <param name="default">A collection of default values to store in the parameter Persistent Data.</param>
    public int AddIntervalParameter(string name, string nickname, string description, GH_ParamAccess access, IEnumerable<Interval> @default)
    {
        Param_Interval param_Interval = new Param_Interval();
        FixUpParameter(param_Interval, name, nickname, description);
        param_Interval.Access = access;
        param_Interval.SetPersistentData(@default.ToArray());
        return AddParameter(param_Interval);
    }

    /// <summary>
    /// Add an Interval2D (i.e. uv domain) parameter to the input list of this component.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddInterval2DParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_Interval2D param_Interval2D = new Param_Interval2D();
        FixUpParameter(param_Interval2D, name, nickname, description);
        param_Interval2D.Access = access;
        return AddParameter(param_Interval2D);
    }

    /// <summary>
    /// Add a Script Variable parameter to the input list of this component. 
    /// Script variable parameters are used almost exclusively in VB/C#/Python components so you probably don't want this one.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddScriptVariableParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_ScriptVariable param_ScriptVariable = new Param_ScriptVariable();
        FixUpParameter(param_ScriptVariable, name, nickname, description);
        param_ScriptVariable.Access = access;
        return AddParameter(param_ScriptVariable);
    }

    /// <summary>
    /// Add a Data Path parameter to the input list of this component. 
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddPathParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_StructurePath param_StructurePath = new Param_StructurePath();
        FixUpParameter(param_StructurePath, name, nickname, description);
        param_StructurePath.Access = access;
        return AddParameter(param_StructurePath);
    }

    /// <summary>
    /// Add a Generic parameter to the input list of this component.
    /// Generic parameters can handle all types of data.
    /// </summary>
    /// <param name="name">The name of the parameter. Keep it short, single words are best.</param>
    /// <param name="nickname">The nickname of the parameter. Keep it short, single characters are best.</param>
    /// <param name="description">The description of the parameter. Be succinct but clear, single sentences are best.</param>
    /// <param name="access">Parameter access type. You must provide a correct access code in order to use GetData(), GetDataList() or GetDataTree() respectively.</param>
    public int AddGenericParameter(string name, string nickname, string description, GH_ParamAccess access)
    {
        Param_GenericObject param_GenericObject = new Param_GenericObject();
        FixUpParameter(param_GenericObject, name, nickname, description);
        param_GenericObject.Access = access;
        return AddParameter(param_GenericObject);
    }
}
#endif
