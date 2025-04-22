using Grasshopper2.Data;
using PancakeNextCore.GH;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;

namespace PancakeNextCore.Utility;

public static class NodeQuery
{
    public static bool TryGetNodeValue(INodeQueryReadCapable root, string[]? path, out IPear? value)
    {
        if (path is null || path.Length == 0)
        {
            throw new ArgumentException(nameof(path));
        }

        var curNode = root;

        for (var i = 0; i < path.Length - 1; i++)
        {
            if (!curNode.TryGetNode(path[i], out var nextNode))
            {
                value = null;
                return false;
            }

            curNode = nextNode;
            if (curNode == null)
            {
                value = null;
                return false;
            }
        }

        return curNode.TryGetContent(path[path.Length - 1], out value);
    }

    public static bool TrySetNodeValue(INodeQueryWriteCapable root, string[]? path, IPear? value, bool addMode = false)
    {
        if (path is null || path.Length == 0)
        {
            throw new ArgumentException(nameof(path));
        }

        var curNode = root;

        for (var i = 0; i < path.Length - 1; i++)
        {
            if (!curNode.TryGetNode(path[i], out var nextNode, true))
            {
                return false;
            }

            curNode = nextNode;
            if (curNode == null)
            {
                return false;
            }
        }

        if(addMode)
        {
            return curNode.AddContent(path[path.Length - 1], value);
        }
        else
        {
            return curNode.SetContent(path[path.Length - 1], value);
        }
    }

    public static IEnumerable<KeyValuePair<string, IPear?>> EnumerateNode(
        INodeQueryReadCapable? root, string delimiter = "/", int depthLimit = 0)
    {
        return EnumerateNode(root, delimiter, "", depthLimit, 1);
    }

    private static IEnumerable<KeyValuePair<string, IPear?>> EnumerateNode(
        INodeQueryReadCapable? root, string delimiter, string currentPath, int depthLimit, int currentDepth)
    {
        if (root is null) yield break;

        foreach (var it in root.GetAttributes())
        {
            yield return new KeyValuePair<string, IPear?>(
                MergePath(currentPath, delimiter, it.Key), it.Value);
        }

        if (depthLimit != 0 && currentDepth >= depthLimit)
        {
            foreach (var it in root.GetNodes())
            {
                yield return new KeyValuePair<string, IPear?>(MergePath(currentPath, delimiter, it.Key), it.Value.AsPear());
            }
        }
        else
        {
            foreach (var it in root.GetNodes())
            {
                foreach (var it2 in EnumerateNode(it.Value,
                    delimiter, MergePath(currentPath, delimiter, it.Key), depthLimit, currentDepth + 1))
                    yield return it2;
            }
        }
    }

    private static string MergePath(string currentPath, string delimiter, string name)
    {
        if (currentPath.Length == 0 || currentPath == null)
            return name ?? "";

        if (currentPath.EndsWith(delimiter))
        {
            return currentPath + name;
        }
        else
        {
            return currentPath + delimiter + name;
        }
    }

}
