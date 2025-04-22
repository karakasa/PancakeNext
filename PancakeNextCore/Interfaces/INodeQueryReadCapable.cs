using Grasshopper2.Data;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PancakeNextCore.Interfaces;

public interface INodeQueryReadCapable
{
    bool TryGetNode(string name, [NotNullWhen(true)] out INodeQueryReadCapable? node);
    bool TryGetContent(string attributeName, [NotNullWhen(true)] out IPear? content);

    IEnumerable<KeyValuePair<string, INodeQueryReadCapable?>> GetNodes();
    IEnumerable<KeyValuePair<string, IPear?>> GetAttributes();

    IEnumerable<string> GetNodeNames();
    IEnumerable<string> GetAttributeNames();
}
