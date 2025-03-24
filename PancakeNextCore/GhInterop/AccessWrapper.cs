using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI;
using Grasshopper2.UI.ErrorFeedback;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GhInterop;

internal static class AccessWrapper
{
    private static bool _failed = false;
    private static PropertyInfo? _nomen = null;
    private static PropertyInfo? _access = null;
    private static void EnsureReflection()
    {
        if (_failed) return;

        _nomen = typeof(DocumentObject)
            .GetProperty("Nomen", BindingFlags.Instance | BindingFlags.Public);

        _access = typeof(AbstractParameter)
            .GetProperty("Access", BindingFlags.Instance | BindingFlags.Public);

        if (_nomen is null || _access is null)
            _failed = true;

        if (_failed)
        {
            PopupErrorMessage();
        }
    }

    private static void PopupErrorMessage()
    {
        var err = new Error("Pancake fail to load.")
        {
            Explanation = "[Reflection error] Nomen"
        };
        err.ShowModal();
    }

    public static void SetNomenByReflection(this DocumentObject docObj, Nomen newName)
    {
        EnsureReflection();

        if (_nomen is null) throw new InvalidOperationException("Fail to set DocumentObject.Nomen. Method unavailable.");
        _nomen.SetValue(docObj, newName);
    }
    static void SetAccessByReflection(this AbstractParameter docObj, Access newAccess)
    {
        EnsureReflection();

        if (_access is null) throw new InvalidOperationException("Fail to set Parameter.Access. Method unavailable.");
        _access.SetValue(docObj, newAccess);
    }

    static TParam CreateParamFallback<TParam>(string name, string nickname, string desc, Access access = Access.Item, Requirement requirement = Requirement.MustExist)
        where TParam : AbstractParameter, new()
    {
        var param = new TParam
        {
            Requirement = requirement,
            UserName = nickname
        };

        param.ModifyNameAndInfo(name, desc);

        // Access doesn't have a public setter (designed to be set by non-default ctor)
        // Find out a better way to change Access.
        param.SetAccessByReflection(access);

        return param;
    }

    static TParam? CreateParamFrequentlyUsed<TParam>(string name, string nickname, string desc, Access access = Access.Item, Requirement requirement = Requirement.MustExist)
        where TParam : AbstractParameter, new()
    {
        var type = typeof(TParam);
        if (type == typeof(IntegerParameter))
        {
            return (TParam)(object)(new IntegerParameter(name, nickname, desc, access) { Requirement = requirement });
        }
        if (type == typeof(NumberParameter))
        {
            return (TParam)(object)(new NumberParameter(name, nickname, desc, access) { Requirement = requirement });
        }
        if (type == typeof(TextParameter))
        {
            return (TParam)(object)(new TextParameter(name, nickname, desc, access) { Requirement = requirement });
        }
        if (type == typeof(BooleanParameter))
        {
            return (TParam)(object)(new BooleanParameter(name, nickname, desc, access) { Requirement = requirement });
        }
        if (type == typeof(GenericParameter))
        {
            return (TParam)(object)(new GenericParameter(name, nickname, desc, access) { Requirement = requirement });
        }

        return null;
    }
    public static TParam CreateParam<TParam>(string name, string nickname, string desc, Access access = Access.Item, Requirement requirement = Requirement.MustExist)
        where TParam : AbstractParameter, new()
    {
        var p = CreateParamFrequentlyUsed<TParam>(name, nickname, desc, access, requirement);
        if (p is not null) return p;

        var type = typeof(TParam);
        if (!_ctors.TryGetValue(type, out var ctor))
        {
            _ctors[type] = ctor = LookForParamConstructor(type);
        }

        if (ctor is null)
        {
            return CreateParamFallback<TParam>(name, nickname, desc, access, requirement);
        }

        p = (TParam)ctor.Invoke([new Nomen(name, desc), access]);
        p.Requirement = requirement;
        p.UserName = nickname;
        return p;
    }

    static ConstructorInfo? LookForParamConstructor(Type type)
    {
        return type.GetConstructor([typeof(Nomen), typeof(Access)]);
    }

    static readonly Dictionary<Type, ConstructorInfo?> _ctors = [];
}
