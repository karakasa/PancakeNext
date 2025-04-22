using Grasshopper2.Data;
using Grasshopper2.Types;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
internal sealed class TupleWrapper(NTuple tuple) : INodeQueryReadCapable
{
    private readonly NTuple _tuple = tuple;

    public IEnumerable<string> GetAttributeNames()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<KeyValuePair<string, IPear?>> GetAttributes()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetNodeNames()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<KeyValuePair<string, INodeQueryReadCapable?>> GetNodes()
    {
        throw new NotImplementedException();
    }

    public bool TryGetContent(string attributeName, [NotNullWhen(true)] out IPear? content)
    {
        throw new NotImplementedException();
    }

    public bool TryGetNode(string name, [NotNullWhen(true)] out INodeQueryReadCapable? node)
    {
        throw new NotImplementedException();
    }
}
