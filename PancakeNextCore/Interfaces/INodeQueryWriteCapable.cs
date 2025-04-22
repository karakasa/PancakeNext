using Grasshopper2.Data;

namespace PancakeNextCore.Interfaces;

public interface INodeQueryWriteCapable
{
    bool TryGetNode(string name, out INodeQueryWriteCapable? node, bool createIfNotExist);
    bool SetContent(string attributeName, IPear? content);
    bool AddContent(string attributeName, IPear? content);
}
