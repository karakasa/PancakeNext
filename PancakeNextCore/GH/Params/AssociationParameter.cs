using Grasshopper2.Parameters;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI;
using GrasshopperIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;

[IoId("5442B78E-DC82-4EE2-9887-0C77FF395F9A")]
public sealed class AssociationParameter : Parameter<GhAssocBase>
{
    public AssociationParameter(IReader reader) : base(reader)
    {
    }

    public AssociationParameter() : base(new Nomen("Association", "Association", "Pancake", "", 0, Rank.Hidden), Access.Tree)
    {
    }

    public override ITypeAssistant<GhAssocBase> TypeAssistant => AssociationTypeAssistant.Instance;
}
