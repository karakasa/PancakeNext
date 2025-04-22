using Grasshopper2.Parameters;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;

[IoId("5442B78E-DC82-4EE2-9887-0C77FF395F9A")]
[ComponentCategory("data", 0)]
public sealed class AssociationParameter : PancakeParameter<GhAssocBase, AssociationParameter>, IPancakeLocalizable<AssociationParameter>
{
    public AssociationParameter(IReader reader) : base(reader) { }
    public AssociationParameter() { }
    public AssociationParameter(Nomen nomen, Access access) : base(nomen, access) { }
    public static string StaticLocalizedName => "Association";
    public static string StaticLocalizedDescription => "Represents a collection of data. Similar to Tuple with fewer limitations.";
    public override ITypeAssistant<GhAssocBase> TypeAssistant => AssociationTypeAssistant.Instance;
}
