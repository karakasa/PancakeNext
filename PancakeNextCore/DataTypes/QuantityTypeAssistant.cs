using Grasshopper2.Data;
using Grasshopper2.Types.Assistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataTypes;
public sealed class QuantityTypeAssistant : TypeAssistant<Quantity>
{
    internal static readonly AssociationTypeAssistant Instance = new();
    public QuantityTypeAssistant() : base("Quantity")
    {
    }
    public override Quantity Copy(Quantity instance) => instance.Duplicate();

    public override string DescribePrimary(Pear<Quantity> pear) => pear.Item?.ToString() ?? "<empty>";
}
