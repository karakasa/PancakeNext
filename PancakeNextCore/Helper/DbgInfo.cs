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
using Grasshopper2.Framework;
using Path = System.IO.Path;
using PancakeNextCore.Utility;
using System.Diagnostics.CodeAnalysis;

namespace PancakeNextCore.Helper;

internal static class DbgInfo
{
    private sealed class PluginEqualityComparer : IEqualityComparer<Plugin>
    {
        public bool Equals(Plugin? x, Plugin? y) => x?.Id == y?.Id;
        public int GetHashCode([DisallowNull] Plugin obj) => obj.Id.GetHashCode();
    }
    public static IEnumerable<ObjectProxy> GetCoreComponents()
    {
        var corePlugins = PluginServer.CorePlugins.Select(FileIo.GetInvariantName).ExcludeNulls().ToHashSet();
        var cache = new ParameterizedCache<HashSet<string>, Plugin, bool>(corePlugins, IsCorePlugin, new PluginEqualityComparer());
        foreach (var proxy in ObjectProxies.Proxies)
        {
            if (cache[proxy.Plugin])
                yield return proxy;
        }
    }

    private static bool IsCorePlugin(HashSet<string> set, Plugin plugin)
    {
        var name = plugin.Location.GetInvariantName();
        if (name is "grasshopper2") return true;
        if (name is null) return false;
        return set.Contains(name);
    }

    internal static void ShowDocObjInfo(IDocumentObject? obj)
    {
        if (obj is null) return;

        try
        {
            var report = "";

            var type = obj.GetType();

            report += DescribeNomen(obj) + "\r\n\r\n";
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
    private static Type? InferInnerType(IParameter param)
    {
        if (param is GenericParameter)
        {
            return typeof(object);
        }

        return param.TypeAssistantWeak?.Type ?? GetGenericParameter(param);
    }
    private static Type? GetGenericParameter(IParameter param)
    {
        return param.GetType().GetInterfaces()
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IParameter<>))
            .Select(t => t.GetGenericArguments()[0])
            .FirstOrDefault();
    }

    private static string DescribeNomen(IDocumentObject obj)
    {
        var nomen = obj.Nomen;
        var str = nomen.Name;
        if (!string.IsNullOrEmpty(obj.UserName))
            str += $" ({obj.UserName})";

        if (!string.IsNullOrEmpty(nomen.Chapter) || !string.IsNullOrEmpty(nomen.Section))
            str += $" [{nomen.Chapter}: {nomen.Section}]";

        return str;
    }

    private static string GetParamDescription(IParameter param)
    {
        if (param == null)
            return "";

        var report =
            $"    {DescribeNomen(param)}\r\n" +
            string.Format(Strings.DbgInfo_GetParamDescription_ExpectedType, InferInnerType(param)?.Name ?? "<unknown>");

        var doc = param.Document;

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
