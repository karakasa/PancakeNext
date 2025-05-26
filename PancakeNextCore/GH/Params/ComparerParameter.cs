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
[IoId("5442B78E-DC82-4EE2-9887-0C77FF395FCC")]
[ComponentCategory("data", 0, Rank.Hidden)]
public sealed class ComparerParameter : PancakeParameter<ICustomComparer, ComparerParameter>, IPancakeLocalizable<ComparerParameter>
{
    public ComparerParameter(IReader reader) : base(reader) { }
    public ComparerParameter() { AddPresets(this); }
    public ComparerParameter(Nomen nomen, Access access) : base(nomen, access) { AddPresets(this); }
    public static string StaticLocalizedName => "Comparer";
    public static string StaticLocalizedDescription => "XXX"; // TODO
    // public override ITypeAssistant<ICustomComparer> TypeAssistant => AssociationTypeAssistant.Instance;

    internal static void AddPresets(Parameter<ICustomComparer> p)
    {
        var presets = p.Presets;
        presets.Clear();
        presets.Add("Default ↑", "The default Pancake order, similar to Grasshopper built-in.", CustomComparer.DefaultAscending);
        presets.Add("Default ↓", "The default Pancake order, similar to Grasshopper built-in, in reversed order.", CustomComparer.DefaultDescending);
        presets.Add("Natural ↑", "Natural sort order. Keys must be texts or will be converted to texts.", CustomComparer.NaturalAscending);
        presets.Add("Natural ↓", "Natural sort order. Keys must be texts or will be converted to texts, in reversed order.", CustomComparer.NaturalDescending);
    }
}
