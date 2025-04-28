using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public class ParameterWrapper : ActiveObjectWrapper<IParameter>, IGH_Param
{
    protected ParameterWrapper(IParameter p) : base(p)
    {
    }
    public static ParameterWrapper CreateFrom(IParameter p) => p switch
    {
        BooleanParameter bp => new Param_Boolean(bp),
        Point3Parameter pp => new Param_Point(pp),
        VectorParameter vp => new Param_Vector(vp),
        TransformParameter tp => new Param_Transform(tp),
        PlaneParameter plp => new Param_Plane(plp),
        BoxParameter bp => new Param_Box(bp),
        LineParameter lp => new Param_Line(lp),
        CircleParameter cp => new Param_Circle(cp),
        RectangleParameter rp => new Param_Rectangle(rp),
        ArcParameter ap => new Param_Arc(ap),
        MeshParameter mp => new Param_Mesh(mp),
        MeshFacetParameter mfp => new Param_MeshFace(mfp),
        IntegerParameter ip => new Param_Integer(ip),
        NumberParameter np => new Param_Number(np),
        DateTimeParameter dtp => new Param_Time(dtp),
        TextParameter tp => new Param_Text(tp),
        IntervalParameter ip => new Param_Interval(ip),
        ColourParameter cp => new Param_Colour(cp),
        PathParameter pp => new Param_StructurePath(pp),
        CurveParameter cp => new Param_Curve(cp),
        SurfaceParameter sp => new Param_Surface(sp),
        _ => new ParameterWrapper(p)
    };
    IParameter IGH_Param.UnderlyingObject => _value;
    public GH_ParamKind Kind => _value.Kind switch
    {
        Grasshopper2.Parameters.Kind.Floating => GH_ParamKind.floating,
        Grasshopper2.Parameters.Kind.Input => GH_ParamKind.input,
        Grasshopper2.Parameters.Kind.Output => GH_ParamKind.output,
        _ => throw new NotSupportedException($"{_value.Kind} cannot be mapped into a GH1 param kind.")
    };

    public Type Type => _value.TypeAssistantWeak?.Type ?? GetGenericType();
    private Type? GetGenericType()
    {
        var type = _value.GetType();
        if (!type.IsGenericTypeDefinition) return null;
        while (type != null && type != typeof(AbstractParameter) && type != typeof(object))
        {
            if (type.GetGenericTypeDefinition() == typeof(Parameter<>))
            {
                var generic = type.GetGenericArguments()[0];
                return generic;
            }
            type = type.BaseType;
        }

        return null;
    }

    public string TypeName => _value.TypeAssistantWeak?.Name ?? GetGenericType()?.Name ?? "<>";

    public bool Optional
    {
        get => _value.Requirement == Requirement.MayBeMissing;
        set => _value.Requirement = value ? Requirement.MayBeMissing : Requirement.MayBeNull;
    }

    public GH_ParamAccess Access
    {
        get => _value.Access switch
        {
            Grasshopper2.Parameters.Access.Item => GH_ParamAccess.item,
            Grasshopper2.Parameters.Access.Twig => GH_ParamAccess.list,
            Grasshopper2.Parameters.Access.Tree => GH_ParamAccess.tree,
            _ => throw new NotSupportedException($"{_value.Kind} cannot be mapped into a GH1 access.")
        };
        set
        {
            if (value == this.Access) return;

            EnsureReflection();
            _access.Invoke(_value, [value]);
        }
    }

    private static bool _reflectionDone = false;
    private static MethodInfo? _access;
    private static void EnsureReflection()
    {
        if (_reflectionDone)
        {
            if (_access is null)
                throw new InvalidOperationException("Cannot find AbstractParameter.set_Access().");
            else
                return;
        }

        _access = typeof(AbstractParameter).GetProperty("Access", 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.SetMethod;
        _reflectionDone = true;
    }

    private static NotSupportedException NotAbstractParameter() => new("Underlying object is not an AbstractParameter.");

    public void RemoveEffects()
    {
        _value.Modifiers = Modifiers.Empty;
    }

    public void AddSource(IGH_Param source)
    {
        Connections.Connect(source.UnderlyingObject, _value);
    }

    public void AddSource(IGH_Param source, int index)
    {
        Connections.Connect(source.UnderlyingObject, _value, int.MaxValue, index);
    }

    public void RemoveSource(IGH_Param source)
    {
        Connections.Disconnect(source.UnderlyingObject, _value);
    }

    private static IParameter Find(Grasshopper2.Doc.Document doc, Guid id)
    {
        return doc.Objects.Find(id) as IParameter ?? throw new KeyNotFoundException($"{id} param not found.");
    }

    public void RemoveSource(Guid source_id)
    {
        var doc = _value.Document;
        var from = Find(doc, source_id);

        Connections.Disconnect(from, _value);
    }

    public void RemoveAllSources()
    {
        Connections.DisconnectAllInputs(_value);
    }

    public void ReplaceSource(IGH_Param old_source, IGH_Param new_source)
    {
        Connections.ReplaceSource(old_source.UnderlyingObject, new_source.UnderlyingObject, _value);
    }

    public void ReplaceSource(Guid old_source_id, IGH_Param new_source)
    {
        var doc = _value.Document;
        var from = Find(doc, old_source_id);

        Connections.ReplaceSource(from, new_source.UnderlyingObject, _value);
    }

    public IList<IGH_Param> Sources
    {
        get
        {
            if (_value is not AbstractParameter ap) throw NotAbstractParameter();
            var doc = ap.Document;
            return [.. ap.Inputs.Forwards.Select(id => doc.Objects.Find(id)).OfType<IParameter>().Select(i => new ParameterWrapper(i))];
        }
    }

    public int SourceCount
    {
        get
        {
            if (_value is not AbstractParameter ap) throw NotAbstractParameter();
            return ap.Inputs.Count;
        }
    }

    public IList<IGH_Param> Recipients
    {
        get
        {
            if (_value is not AbstractParameter ap) throw NotAbstractParameter();
            var doc = ap.Document;
            return [.. ap.Outputs.Forwards.Select(id => doc.Objects.Find(id)).OfType<IParameter>().Select(i => new ParameterWrapper(i))];
        }
    }

    public int VolatileDataCount => _value.PersistentDataWeak.LeafCount;

    public virtual IGH_Structure VolatileData => _value.PersistentDataWeak.To1();

    public GH_DataMapping DataMapping
    {
        get
        {
            if (_value.Modifiers.Flatten)
                return GH_DataMapping.Flatten;

            if (_value.Modifiers.Graft)
                return GH_DataMapping.Graft;

            return GH_DataMapping.None;
        }
        set
        {
            switch (value)
            {
                case GH_DataMapping.None:
                    if (!_value.Modifiers.Graft && !_value.Modifiers.Flatten) return;
                    _value.Modifiers = _value.Modifiers.WithoutGrafting().WithoutFlatten();
                    break;
                case GH_DataMapping.Flatten:
                    if (_value.Modifiers.Flatten) return;
                    _value.Modifiers = _value.Modifiers.WithoutGrafting().WithFlatten();
                    break;
                case GH_DataMapping.Graft:
                    if (_value.Modifiers.Graft) return;
                    _value.Modifiers = _value.Modifiers.WithGrafting().WithoutFlatten();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(DataMapping));
            }
        }
    }
    public bool Reverse
    {
        get => _value.Modifiers.Reverse;
        set
        {
            if (_value.Modifiers.Reverse == value) return;
            if (value)
                _value.Modifiers = _value.Modifiers.WithReverse();
            else
                _value.Modifiers = _value.Modifiers.WithoutReverse();
        }
    }
    public bool Simplify
    {
        get => _value.Modifiers.Simplify;
        set
        {
            if (_value.Modifiers.Simplify == value) return;
            if (value)
                _value.Modifiers = _value.Modifiers.WithSimplify();
            else
                _value.Modifiers = _value.Modifiers.WithoutSimplify();
        }
    }
}
