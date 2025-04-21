using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PancakeNextCore.Utility;

internal static class ReflectionHelper
{
    public static readonly object[] NoParameter = [];
    public static bool IsSubclassOfRawGeneric(Type generic, Type? toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }
            toCheck = toCheck.BaseType;
        }
        return false;
    }

    public static bool IsForeignMethodAvailable(Type baseObject, string methodName, bool onlyPublic = false)
    {
        MethodInfo? fForeign = null;
        var tParam = baseObject;

        while (tParam != null)
        {
            if (onlyPublic)
                fForeign = tParam.GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            else
                fForeign = tParam.GetMethod(methodName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            if (fForeign != null)
                break;
            tParam = tParam.BaseType;
        }

        return fForeign != null;
    }

    public static MethodInfo? GetForeignMethod(Type? tParam, string methodName, bool onlyPublic = false, Type[]? parameters = null)
    {
        try
        {
            MethodInfo? fForeign = null;

            while (tParam != null)
            {
                BindingFlags flags;

                if (onlyPublic)
                    flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
                else
                    flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

                if (parameters is null)
                {
                    fForeign = tParam.GetMethod(methodName, flags);
                }
                else
                {
                    fForeign = tParam.GetMethod(methodName, flags, null, parameters, null);
                }

                if (fForeign != null)
                    break;
                tParam = tParam.BaseType;
            }

            return fForeign;
        }
        catch
        {
            throw new NotSupportedException($"Fail to locate method {methodName} on {tParam?.FullName}");
        }
    }

    public static object? InvokeForeignMethod(object baseObject, string methodName, object[] Params, bool nullThis = false, bool onlyPublic = false)
    {
        var fForeign = GetForeignMethod(baseObject.GetType(), methodName, onlyPublic);

        if (fForeign != null)
        {
            return fForeign.Invoke(nullThis ? null : baseObject, Params);
        }
        else
        {
            throw new Exception("Method not found");
        }
    }

    internal static PropertyInfo? GetPropertyInternal(IReflect baseObject, string propertyName, bool onlyPublic = false)
    {
        BindingFlags bf;
        if (onlyPublic)
            bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
        else
            bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        return baseObject?.GetProperty(propertyName, bf);
    }

    public static object? GetProperty(object baseObject, string propertyName, bool onlyPublic = false, bool staticDef = false)
    {
        return GetPropertyInternal(baseObject.GetType(), propertyName, onlyPublic)?.GetValue(
            staticDef ? null : baseObject,
            null);
    }

    public static void SetProperty(object baseObject, string propertyName, object value, bool onlyPublic = false)
    {
        GetPropertyInternal(baseObject.GetType(), propertyName, onlyPublic)?.SetValue(baseObject, value);
    }

    public static object IsPropertyAvailable(object baseObject, string propertyName, bool onlyPublic = false)
    {
        return null != GetPropertyInternal(baseObject.GetType(), propertyName, onlyPublic);
    }

    internal static FieldInfo? GetFieldInternal(IReflect baseObject, string propertyName, bool onlyPublic = false)
    {
        BindingFlags bf;
        if (onlyPublic)
            bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
        else
            bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        return baseObject?.GetField(propertyName, bf);
    }

    public static object? GetField(object baseObject, string propertyName, bool onlyPublic = false)
    {
        return GetFieldInternal(baseObject.GetType(), propertyName, onlyPublic)?.GetValue(baseObject);
    }

    public static void SetField(object baseObject, string propertyName, object value, bool onlyPublic = false)
    {
        GetFieldInternal(baseObject.GetType(), propertyName, onlyPublic)?.SetValue(baseObject, value);
    }

    public static object IsFieldAvailable(object baseObject, string propertyName, bool onlyPublic = false)
    {
        return null != GetFieldInternal(baseObject.GetType(), propertyName, onlyPublic);
    }

    public static IEnumerable<T> GetEnumerableOfType<T>(Assembly? assembly = null) where T : class
    {
        var objects = RetrieveAssemblyTypes<T>(assembly)
            .Select(type => (T)Activator.CreateInstance(type)!
            );
        return objects;
    }

    public static IEnumerable<Type> RetrieveAssemblyTypes<T>(Assembly? assembly = null) where T : class
    {
        return RetrieveAssemblyTypes(typeof(T), assembly);
    }

    public static IEnumerable<Type> RetrieveAssemblyTypes(Type type, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetAssembly(type);
        if (assembly is null) return [];

        return [.. assembly
            .GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && type.IsAssignableFrom(myType) && (assembly is null || myType.IsPublic))];
    }
    public static List<Type> GetTypeChain(Type srcType)
    {
        var list = new List<Type>();

        var curType = srcType;
        for (; ; )
        {
            list.Add(curType);

            curType = curType.BaseType;
            if (curType == null || curType == typeof(object) || curType == typeof(Enum) || curType == typeof(ValueType))
            {
                break;
            }

            if (list.Count > 1000)
            {
                throw new InvalidOperationException("Recursive issue at ReflectionHelper. Code: 1");
            }
        }

        list.Reverse();
        return list;
    }

    public static Func<TParentType, TFieldType>? BuildFieldAccessor<TParentType, TFieldType>(string fieldName)
    {
        var field = typeof(TParentType).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field is null)
            // Cannot find the field
            return null;

        if (typeof(TFieldType) != field.FieldType || !typeof(TFieldType).IsAssignableFrom(field.FieldType))
            // Field type non-convertible
            return null;

        var param = Expression.Parameter(typeof(TParentType), "o");
        var fieldAccessor = Expression.Field(param, fieldName);
        var lambda = Expression.Lambda(typeof(Func<TParentType, TFieldType>), fieldAccessor, param);
        var compliedFunction = (Func<TParentType, TFieldType>)lambda.Compile();

        return compliedFunction;
    }
    public static object? GetStaticProperty<T>(string name)
    {
        return typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
    }
    public static EventHandler<TArgs>? GetStaticEventHandler<TArgs>(this Type clsType, string eventName)
    {
        return clsType.GetField(eventName, BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as EventHandler<TArgs>;
    }
    public static EventHandler<TArgs>? GetEventHandler<TArgs>(this object @class, string eventName)
    {
        return @class.GetType().GetField(eventName, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(@class) as EventHandler<TArgs>;
    }

    public static string Describe(this Delegate del)
    {
        return $"{del.Method?.ToString() ?? "<>"} on {del.Target?.GetType()?.FullName ?? "<>"}";
    }
}
