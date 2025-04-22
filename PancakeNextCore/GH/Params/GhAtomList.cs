using Grasshopper2.Data;
using GrasshopperIO;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using PancakeNextCore.Utility.Polyfill;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PancakeNextCore.GH.Params;
[IoId("22B79783-C674-4BC4-AFBA-014C94D727BE")]
public sealed class GhAtomList : GhAssocBase
{
    public GhAtomList()
    {
    }
    public GhAtomList(IReader reader) : base(reader)
    {
    }

    public GhAtomList(object?[] objects)
    {
        EnsureData(objects.Length);
        Values.AddRange(objects.AsPears());
    }

    public GhAtomList(ITwig twig)
    {
        EnsureData(twig.ItemCount);
        Values.AddRange(twig.Pears);
    }

    public GhAtomList(IPear?[] pears)
    {
        EnsureData(pears.Length);
        Values.AddRange(pears);
    }

    internal override GhAssocBase GenericClone()
    {
        return new GhAtomList()
        {
            Values = HasValues ? new(Values) : null
        };
    }

    public void Add(object? obj)
    {
        Add(obj.AsPear());
    }

    public void Add(IPear? obj)
    {
        EnsureData(0);
        Values.Add(obj);
    }

    public override IEnumerable<string> GetNamesForExport()
    {
        if (!HasValues) yield break;

        for (var i = 0; i < Values.Count; i++)
        {
            yield return i.ToString(CultureInfo.InvariantCulture);
        }
    }

    internal IEnumerable<object?> GetFlattenInnerListUnwrapped()
    {
        if (!HasValues) yield break;

        foreach (var pear in Values)
        {
            var it = pear?.Item;

            if (it is GhAtomList list)
            {
                foreach (var it2 in list.GetFlattenInnerListUnwrapped())
                    yield return it2;
            }
            else
            {
                yield return it;
            }
        }
    }

    private static bool TryGetIndex(string name, out int index)
    {
        if (name.Length == 0) { index = 0; return false; }
        return name[0] == '@' ? name.TryParseSubstrAsInt(1, out index) : name.TryParseSubstrAsInt(0, out index);
    }
    public override bool TryGetContent(string name, out IPear? output)
    {
        if (!HasValues)
        {
            output = null;
            return false;
        }

        if (TryGetIndex(name, out var indice))
        {
            if (!IsOutOfRange(indice))
            {
                output = Values[indice];
                return true;
            }
        }

        output = null;
        return false;
    }

    public override bool TryGetNode(string name, out INodeQueryReadCapable? node)
    {
        TryGetNode(name, out var node2, false);
        node = node2 as INodeQueryReadCapable;
        return node != null;
    }

    public override bool TryGetNode(string name, out INodeQueryWriteCapable? node, bool createIfNotExist)
    {
        if (!TryGetContent(name, out var obj))
        {
            node = null;
            return false;
        }

        if (obj is INodeQueryWriteCapable nodeout)
        {
            node = nodeout;
            return true;
        }

        if (createIfNotExist)
        {
            var newNode = new GhAssoc();
            if (SetContent(name, newNode.AsPear()))
            {
                node = newNode;
                return true;
            }
            else
            {
                throw new InvalidOperationException("In-place creation beyond list size is not supported with AtomList.");
            }
        }

        node = null;
        return false;
    }

    public override bool SetContent(string attributeName, IPear? content)
    {
        if (!TryGetIndex(attributeName, out var id))
        {
            return false;
        }

        if (!HasValues)
        {
            if (id == 0)
            {
                Add(content);
                return true;
            }
            else
            {
                return false;
            }
        }

        if (id < 0 || id > Values.Count)
            return false;

        if (id == Values.Count)
        {
            Add(content);
        }
        else
        {
            Values[id] = content;
        }

        return true;
    }

    public override IEnumerable<KeyValuePair<string, INodeQueryReadCapable?>> GetNodes()
    {
        if (!HasValues) yield break;

        for (var i = 0; i < Values.Count; i++)
        {
            if (Values[i] is INodeQueryReadCapable inode)
            {
                yield return new KeyValuePair<string, INodeQueryReadCapable?>(i.ToString(), inode);
            }
        }
    }

    public override IEnumerable<KeyValuePair<string, IPear?>> GetAttributes()
    {
        if (!HasValues) yield break;

        for (var i = 0; i < Values.Count; i++)
        {
            var it = Values[i];
            if (it is not INodeQueryReadCapable)
            {
                yield return new KeyValuePair<string, IPear?>(i.ToString(), it);
            }
        }
    }

    public override bool AddContent(string attributeName, IPear? content)
    {
        return SetContent(attributeName, content);
    }
    public override IEnumerable<string> GetNodeNames() => Enumerable.Empty<string>();

    public override IEnumerable<string> GetAttributeNames() => Enumerable.Empty<string>();
}
