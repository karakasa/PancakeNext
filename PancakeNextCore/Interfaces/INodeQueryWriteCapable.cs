using Grasshopper2.Data;
using System.Diagnostics.CodeAnalysis;

namespace PancakeNextCore.Interfaces;

public interface INodeQueryWriteCapable
{
    bool TryGetNode(string name, [NotNullWhen(true)] out INodeQueryWriteCapable? node, bool createIfNotExist);
    bool SetContent(string attributeName, IPear? content);
    bool AddContent(string attributeName, IPear? content);
}
