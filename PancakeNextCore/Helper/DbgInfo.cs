using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grasshopper2.Doc;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using GrasshopperIO;
using PancakeNextCore.UI;
using Grasshopper2.Parameters.Standard;

namespace PancakeNextCore.Helper;

/// <summary>
/// Debug info class. These features will soon be moved to another plugin.
/// Do not translate this module.
/// </summary>
internal class DbgInfo
{
    /*public static string[] GetCoreComponents()
    {
        var list = new List<string>();
        var coreAssembly = new HashSet<Guid>();
        var noncoreAssembly = new HashSet<Guid>();

        var objs = Instances.ComponentServer.ObjectProxies;

        foreach (var obj in objs)
        {
            if (obj.Kind != GH_ObjectType.CompiledObject)
            {
                continue;
            }

            var assemblyGuid = obj.LibraryGuid;
            if (coreAssembly.Contains(assemblyGuid))
            {
                list.Add($"{obj.Guid},{obj.Desc.Name},{obj.Type.FullName}");
                continue;
            }
            if (noncoreAssembly.Contains(assemblyGuid))
            {
                continue;
            }

            var assembly = Instances.ComponentServer.FindAssembly(assemblyGuid);
            if (assembly == null)
            {
                noncoreAssembly.Add(assemblyGuid);
                continue;
            }

            var isCore = assembly.IsCoreLibrary;
            (isCore ? coreAssembly : noncoreAssembly).Add(assemblyGuid);
            if (isCore)
            {
                list.Add($"{obj.Guid},{obj.Desc.Name},{obj.Type.FullName}");
            }
        }

        return list.ToArray();
    }*/

    internal static void ShowDocObjInfo(IDocumentObject? obj)
    {
        if (obj is null) return;

        try
        {
            var report = "";

            var type = obj.GetType();

            report += DescribeNomen(obj) + "\r\n";
            report += GetTypeDescription(type);
            report += string.Format(Strings.DbgInfo_ShowDocObjInfo_CompId, obj.GetType().GetCustomAttribute<IoIdAttribute>()?.Id ?? Guid.Empty);
            report += string.Format(Strings.DbgInfo_ShowDocObjInfo_InstId, obj.InstanceId);
            report += "\r\n";

            var interfaceList = string.Join("\r\n    ",
                type.GetInterfaces().Except(typeof(DocumentObject).GetInterfaces()));

            report += string.Format(Strings.DbgInfo_ShowDocObjInfo_Implement, interfaceList);
            report += Strings.DbgInfo_ShowDocObjInfo_Inherited_from__;
            var baseType = type.BaseType;
            while (baseType != null && baseType != typeof(DocumentObject))
            {
                report += "\r\n";
                report += GetTypeDescription(baseType, 1);
                baseType = baseType.BaseType;
            }

            report += "\r\n";

            switch (obj)
            {
                case Component component:
                    report += Strings.DbgInfo_ShowDocObjInfo_InpParam;
                    foreach (var input in component.Parameters.Inputs)
                    {
                        report += GetParamDescription(input);
                    }

                    report += Strings.DbgInfo_ShowDocObjInfo_OutParam;
                    foreach (var input in component.Parameters.Outputs)
                    {
                        report += GetParamDescription(input);
                    }

                    break;

                case IParameter param:
                    report += Strings.DbgInfo_ShowDocObjInfo_ParamInfo;
                    report += GetParamDescription(param);
                    break;
            }

            Presenter.ShowReportWindow(report);
        }
        catch (Exception e)
        {
            var report = Strings.DbgInfo_ShowDocObjInfo_Exception;
            report += e.ToString();
            Presenter.ShowReportWindow(report);
        }

    }
    private static Type? TryDetermineInnerType(IParameter param)
    {
        if (param is GenericParameter)
        {
            return typeof(object);
        }

        return param.TypeAssistantWeak?.Type ?? GetGenericInterfaceImplementation(param);
    }
    private static Type? GetGenericInterfaceImplementation(IParameter param)
    {
        return param.GetType().GetInterfaces()
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IParameter<>))
            .Select(t => t.GetGenericArguments()[0])
            .FirstOrDefault();
    }

    private static string DescribeNomen(IDocumentObject obj)
    {
        var nomen = obj.Nomen;
        return $"{nomen.Name} ({obj.UserName ?? "/"}) from {nomen.Chapter}-{nomen.Section}";
    }

    private static string GetParamDescription(IParameter param)
    {
        if (param == null)
            return "";

        var report =
            $"    {DescribeNomen(param)}\r\n" +
            string.Format(Strings.DbgInfo_GetParamDescription_ExpectedType, TryDetermineInnerType(param)?.Name ?? "<unknown>");

        try
        {
            /*
            if (param.Kind is Kind.Input or Kind.Floating)
            {
                var typeTable = new HashSet<Type>();
                foreach (var single in param.Inputs)
                {
                    var atom = single.VolatileData;
                    if (atom?.PathCount != 0)
                    {
                        var atomList = atom?.get_Branch(0);
                        if (atomList == null || atomList.Count == 0)
                            continue;
                        typeTable.Add(atomList[0].GetType());
                    }
                }

                if (typeTable.Count > 0)
                {
                    report += Strings.DbgInfo_GetParamDescription_SrcType;
                    foreach (var type in typeTable)
                    {
                        report += $"        {type}\r\n";
                    }
                }

                if (param.SourceCount != 0)
                {
                    report += Strings.DbgInfo_GetParamDescription_Srcs;
                    report = param.Sources.Aggregate(report,
                        (current, single) =>
                            current +
                            $"        {single.Attributes.GetTopLevel.DocObject.Name}, {single.NickName}: {single.Name}\r\n");
                }
            }
            if (param.Kind is Kind.Output or Kind.Floating)
            {

                var typeTable = new HashSet<Type>();
                var atom = param.VolatileData;
                if (atom?.PathCount != 0)
                {
                    var atomList = atom?.get_Branch(0);
                    if (atomList != null && atomList.Count != 0)
                    {
                        typeTable.Add(atomList?[0]?.GetType());
                    }
                }

                if (typeTable.Count > 0)
                {
                    report += Strings.DbgInfo_GetParamDescription_OutType;
                    foreach (var type in typeTable)
                    {
                        report += $"        {type}\r\n";
                    }
                }

                if (param.Recipients.Count != 0)
                {
                    report += Strings.DbgInfo_GetParamDescription_Recipients;
                    report = param.Recipients.Aggregate(report,
                        (current, single) =>
                            current +
                            $"        {single.Attributes.GetTopLevel.DocObject.Name}, {single.NickName}: {single.Name}\r\n");
                }
            }
            */
        }
        catch (Exception)
        {
            //
        }

        report += "\r\n";
        return report;
    }

    internal static string GetTypeDescription(Type type, int indent = 0)
    {
        var indentStr = new string('\t', indent).Replace("\t", "    ");
        var report = "";
        report += string.Format(Strings.DbgInfo_GetTypeDescription_FullName, indentStr, type.Namespace, type.Name);
        if (type.IsGenericType)
        {
            object[] gtArgs = type.GetGenericArguments();
            var genericTerms = string.Join(", ", gtArgs);
            report += string.Format(Strings.DbgInfo_GetTypeDescription_Generics, indentStr, genericTerms);
        }
        report += string.Format(Strings.DbgInfo_GetTypeDescription_Defined, indentStr, type.Assembly.FullName);
        try
        {
            var locStr = type.Assembly.Location;
            if (string.IsNullOrEmpty(locStr))
                report += string.Format(Strings.DbgInfo_GetTypeDescription_DynLoad, indentStr);
            else
                report += string.Format(Strings.DbgInfo_GetTypeDescription_Stored, indentStr, locStr);
        }
        catch (NotSupportedException)
        {
            report += string.Format(Strings.DbgInfo_GetTypeDescription_DynLoad, indentStr);
        }

        return report;
    }
}
