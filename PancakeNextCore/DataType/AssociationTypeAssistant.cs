using Grasshopper2.Data;
using Grasshopper2.Types.Assistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataType;
public sealed class AssociationTypeAssistant : TypeAssistant<Association>
{
    internal static readonly AssociationTypeAssistant Instance = new();
    public AssociationTypeAssistant() : base("Association")
    {
    }

    public override Association Copy(Association instance) => instance.GenericClone();

    public override string DescribePrimary(Pear<Association> pear) => pear.Item?.ToString() ?? "<empty>";
    public override int Sort(Association a, Association b)
    {
        throw new NotSupportedException("Association cannot be sorted.");
    }
    public override bool Same(Association a, Association b)
    {
        throw new NotImplementedException();
    }
}
