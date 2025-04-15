using Grasshopper2.Data;
using Grasshopper2.Types.Assistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataType;
public sealed class AssociationTypeAssistant : TypeAssistant<GhAssocBase>
{
    internal static readonly AssociationTypeAssistant Instance = new();
    public AssociationTypeAssistant() : base("Association")
    {
    }

    public override GhAssocBase Copy(GhAssocBase instance) => instance.GenericClone();

    public override string DescribePrimary(Pear<GhAssocBase> pear) => pear.Item?.ToString() ?? "<empty>";
    public override int Sort(GhAssocBase a, GhAssocBase b)
    {
        throw new NotSupportedException("Association cannot be sorted.");
    }
    public override bool Same(GhAssocBase a, GhAssocBase b)
    {
        throw new NotImplementedException();
    }
}
