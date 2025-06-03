using Grasshopper2.Parameters;
using GrasshopperIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
[IoId(GuidString)]
public sealed class ComparerPin : Pin<ICustomComparer>
{
    const string GuidString = "62703525-8603-49CF-A343-8FD4B8086530";
    internal static readonly Guid TypeId = new(GuidString);

    public ComparerPin(IReader reader) : base(reader)
    {
    }
    public ComparerPin() : base(new Grasshopper2.UI.Nomen("Pancake", "Custom Comparer", rank: Grasshopper2.UI.Rank.Hidden))
    {
        ComparerParameter.AddPresets(this);
        UserName = "comparer";
    }

    public override Guid PinTypeId => TypeId;
    internal static readonly Guid[] PinIdSingleton = [TypeId];

    public override void Store(IWriter writer)
    {
        base.Store(writer);
    }
}
