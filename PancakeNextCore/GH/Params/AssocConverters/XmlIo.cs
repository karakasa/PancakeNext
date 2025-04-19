using PancakeNextCore.Utility;
using PancakeNextCore.Utility.Polyfill;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace PancakeNextCore.GH.Params.AssocConverters;

internal sealed class XmlIo
{
    public static bool AllowForMagicContent = true;
    public static bool MergeDuplicatedEntries = true;
    public static string MagicContentMarker = "@";
    public const string Header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
    public const string CDATAHeader = "<![CDATA[";
    public const string CDATAEnd = "]]>";

    private static string GetFirstPart(string name)
    {
        if (name == null)
            return "";

        var index = name.LastIndexOf("@");
        if (index == -1)
            return name;
        else
            return name.Substring(0, index);
    }

    private static int GetSecondPart(string name)
    {
        if (name == null)
            return -1;

        var index = name.LastIndexOf("@");

        if (index == -1 || !name.TryParseSubstrAsInt(index + 1, out var indice))
            return 0;
        else
            return indice;
    }

    public bool Expand = false;
    public bool Headless = false;

    private readonly StringBuilder builder = new();
    public string CreateXml(string rootTag, GhAssocBase node)
    {
        builder.Clear();

        if (!Headless)
            builder.AppendLine(Header);

        CreateXmlFromNode(rootTag, node);

        try
        {
            return builder.ToString();
        }
        finally
        {
            builder.Clear();
        }
    }

    private void WrapInnerText(string? str)
    {
        if (str == null)
        {
            throw new XmlException("Unhandled null value in XML generation.");
        }
        if (str.StartsWith(CDATAHeader) && str.EndsWith(CDATAEnd))
        {
            if (str.IndexOf(CDATAEnd) != str.Length - CDATAEnd.Length)
                throw new XmlException($"There shouldn't be '{CDATAEnd}' in the middle of a CDATA block.");

            builder.Append(str);
        }
        else
        {
            str.EscapeForXml(builder);
        }
    }
    private void CreateXmlFromNode(string tagName, GhAssocBase node)
    {
        var realName = tagName;

        if (tagName == null)
            return;

        var index = realName.IndexOf("@");
        if (index != -1)
            realName = realName.Substring(0, index);

        CheckTagName(realName);

        if (node is GhAtomList list)
        {
            CreateXmlFromNode(realName, list);
        }
        else if (node is GhAssoc na)
        {
            CreateXmlFromNode(realName, na);
        }
    }
    private void CreateXmlFromNode(string realName, GhAtomList list)
    {
        foreach (var innerObj in list.GetFlattenInnerListUnwrapped())
        {
            if (innerObj is GhAssocBase subnode)
            {
                CreateXmlFromNode(realName, subnode);
            }
            else
            {
                builder.Append($"<{realName}>");
                if (innerObj != null)
                    WrapInnerText(innerObj?.ToString());
                builder.AppendLine($"</{realName}>");
            }
        }
    }

    private void CreateXmlFromNode(string realName, GhAssoc na)
    {
        builder.Append($"<{realName}");

        var hasInnerContent = false;
        var attributes = na.GetAttributes().Where(attr => !AllowForMagicContent || attr.Key != MagicContentMarker);

        object? content = null;
        var hasInnerContentDefined = AllowForMagicContent && na.TryGetContent(MagicContentMarker, out content) && content is not GhAssocBase;
        var nodeListArray = na.GetNodes().ToArray();

        if (Expand && !hasInnerContentDefined && nodeListArray.Length == 0)
        {
            foreach (var attr in attributes)
            {
                CheckTagName(attr.Key);
                if (!hasInnerContent)
                {
                    builder.AppendLine(">");
                    hasInnerContent = true;
                }

                builder.Append($"<{attr.Key}>");
                attr.Value?.ToString().EscapeForXml(builder);
                builder.AppendLine($"</{attr.Key}>");
            }
        }
        else
        {
            foreach (var attr in attributes)
            {
                CheckTagName(attr.Key);
                builder.Append($" {attr.Key}=\"");
                attr.Value?.ToString().EscapeForXml(builder);

                builder.Append('"');
            }
        }

        if (hasInnerContentDefined)
        {
            if (!hasInnerContent)
            {
                builder.Append('>');
                hasInnerContent = true;
            }
            if (content != null)
                WrapInnerText(content.ToString());
        }
        else
        {
            var nodeList = nodeListArray.OrderBy(kv => GetFirstPart(kv.Key)).ThenBy(kv => GetSecondPart(kv.Key));

            foreach (var it in nodeList)
            {
                if (!hasInnerContent)
                {
                    hasInnerContent = true;
                    builder.AppendLine(">");
                }

                CreateXmlFromNode(it.Key, it.Value);
            }
        }

        if (hasInnerContent)
        {
            builder.Append($"</{realName}>");
        }
        else
        {
            builder.Append(" />");
        }
        builder.AppendLine();
    }

    public const string ForbiddenTagNameChars = "!\"#$%&'()*+,/;<=>?@[\\]^`{|}~ ";
    public const string ForbiddenStartTagNameChars = "-.0123456789";

    private static void CheckTagName(string name)
    {
        if (!IsValidTagName(name))
            throw new XmlException($"{name} is not a valid XML tag.");
    }
    public static bool IsValidTagName(string name)
    {
        if (name.Length == 0)
            return false;

        if (IsStringAnyCharDisallowed(name))
            return false;

        return !IsDisallowedFirstChar(name[0]);
    }

    private static bool IsStringAnyCharDisallowed(string s)
    {
        foreach (var c in s)
        {
            if (IsDisallowedTagChar(c)) return true;
        }

        return false;
    }
    private static bool IsDisallowedTagChar(char c)
    {
        return c >= ' ' && c <= ',' || c == '/' || c >= ';' && c <= '@' || c >= '[' && c <= '^' || c == '`' || c >= '{' && c <= '~';
    }
    private static bool IsDisallowedFirstChar(char firstChar)
    {
        return firstChar == '-' || firstChar == '.' || firstChar >= '0' && firstChar <= '9';
    }

    public static GhAssocBase ReadXml(string content, out string? root, ICollection<string>? interested = null)
    {
        var file = FileIo.IsValidPath(content);

        var settings = new XmlReaderSettings()
        {
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        using var sreader = new StringReader(content);
        using var xml = file ? XmlReader.Create(content, settings) : XmlReader.Create(sreader, settings);

        var path = new Stack<string>();
        var storage = new Stack<GhAssoc>();
        var rootAssoc = new GhAssoc();
        var filter = ProcessTree.CreateFrom(interested);

        root = null;

        if (!xml.Read())
        {
            return rootAssoc;
        }

        for (; ; )
        {
            var skipRead = false;

            switch (xml.NodeType)
            {
                case XmlNodeType.Element:
                    skipRead = ReadXmlElement(path, storage, rootAssoc, filter, xml, ref root);
                    break;

                case XmlNodeType.Text:
                    storage.Peek().Add("@", xml.ReadContentAsString());
                    skipRead = true;
                    break;

                case XmlNodeType.CDATA:
                    storage.Peek().Add("@", CDATAHeader + xml.ReadContentAsString() + CDATAEnd);
                    skipRead = true;
                    break;

                case XmlNodeType.EndElement:
                    path.Pop();
                    storage.Pop();
                    filter.Return();
                    break;
            }

            if (!skipRead) continue;
            if (!xml.Read()) break;
        }

        return rootAssoc;
    }

    private static bool ReadXmlElement(Stack<string> path, Stack<GhAssoc> storage, GhAssoc rootAssoc, ProcessTree filter,
        XmlReader xml, ref string? root)
    {
        GhAssoc? cur = null;

        if (path.Count == 0)
        {
            root = xml.Name;
            cur = rootAssoc;
        }
        else
        {
            if (!filter.Into(xml.Name))
            {
                xml.Skip();
                return true;
            }

            var assoc = new GhAssoc();
            cur = assoc;

            var currentAssoc = storage.Peek();

            if (MergeDuplicatedEntries)
            {
                var index = 0;
                var found = false;
                foreach (var it in currentAssoc.GetRawNames() ?? [])
                {
                    if (it != null && it == xml.Name)
                    {
                        found = true;
                        break;
                    }

                    ++index;
                }

                if (found)
                {
                    var insideObj = currentAssoc.Get(index);
                    if (insideObj is GhAtomList atomList)
                    {
                        atomList.Add(assoc);
                    }
                    else
                    {
                        currentAssoc.Set(index, new GhAtomList([insideObj, assoc]));
                    }
                }
                else
                {
                    currentAssoc.Add(xml.Name, assoc);
                }
            }
            else
            {
                currentAssoc.Add(xml.Name, assoc);
            }
        }

        storage.Push(cur);
        path.Push(xml.Name);

        if (xml.HasAttributes)
        {
            while (xml.MoveToNextAttribute())
            {
                cur.Add(xml.Name, xml.ReadContentAsString());
            }

            xml.MoveToElement();
        }

        if (xml.IsEmptyElement)
        {
            path.Pop();
            storage.Pop();
            filter.Return();
        }

        return false;
    }

    private sealed class ProcessTree
    {
        public bool AllowEverything = false;
        private readonly Dictionary<string, ProcessTree> _tree = [];
        public static ProcessTree CreateFrom(ICollection<string>? interested)
        {
            if (interested == null || interested.Count == 0)
                return new ProcessTree() { AllowEverything = true };

            var tree = new ProcessTree();

            var empty = true;

            foreach (var it in interested)
            {
                if (!string.IsNullOrEmpty(it)) empty = false;
                tree.AddPath(it.Split('/'));
            }

            if (empty)
                tree.AllowEverything = true;

            return tree;
        }

        public ProcessTree()
        {
            _current = this;
            _stack.Push(this);
        }

        private ProcessTree? _current;
        private readonly Stack<ProcessTree?> _stack = [];
        public bool Into(string name)
        {
            if (AllowEverything)
                return true;

            if (_current == null)
            {
                _stack.Push(null);
                return true;
            }

            if (!_current._tree.TryGetValue(name, out var next))
                return false;

            _stack.Push(_current);
            _current = next;
            return true;
        }

        public void Return()
        {
            if (!AllowEverything)
                _current = _stack.Pop();
        }
        private void AddPath(IList<string> paths)
        {
            var cur = this;

            for (var i = 0; i < paths.Count; i++)
            {
                if (i == paths.Count - 1)
                {
                    cur._tree[paths[i]] = null;
                }
                else
                {
                    var next = new ProcessTree();
                    cur._tree[paths[i]] = next;
                    cur = next;
                }
            }
        }
    }

    public static void CollapseXmlAssoc(GhAssocBase obj)
    {
        for (var i = 0; i < obj.Length; i++)
        {
            switch (obj.Values![i])
            {
                case GhAssoc subAssoc:
                    if (subAssoc.Length == 1 && string.Equals(subAssoc.Names?.FirstOrDefault(), MagicContentMarker))
                    {
                        var val = subAssoc.Values![0];
                        if (val is string str)
                        {
                            obj.Values[i] = str;
                        }
                        else
                        {
                            obj.Values[i] = val;
                        }
                    }
                    else
                    {
                        CollapseXmlAssoc(subAssoc);
                    }
                    break;

                case GhAtomList alist2:
                    CollapseXmlAssoc(alist2);
                    break;

                default:
                    continue;
            }
        }
    }
}
