using Grasshopper2.Parameters;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI;
using GrasshopperIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataTypes;
public sealed class AssociationParameter : Parameter<Association>
{
    public AssociationParameter(IReader reader) : base(reader)
    {
    }

    public AssociationParameter() : base(new Nomen("Association", "Association", "Pancake", "", 0, Rank.Hidden), Access.Tree)
    {
    }

    public override ITypeAssistant<Association> TypeAssistant => AssociationTypeAssistant.Instance;
}
