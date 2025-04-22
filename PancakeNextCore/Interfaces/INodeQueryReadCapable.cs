using Grasshopper2.Data;
using System.Collections.Generic;

namespace PancakeNextCore.Interfaces;

public interface INodeQueryReadCapable
{
    bool TryGetNode(string name, out INodeQueryReadCapable? node);
    bool TryGetContent(string attributeName, out IPear? content);

    IEnumerable<KeyValuePair<string, INodeQueryReadCapable?>> GetNodes();
    IEnumerable<KeyValuePair<string, IPear?>> GetAttributes();

    IEnumerable<string> GetNodeNames();
    IEnumerable<string> GetAttributeNames();
}
