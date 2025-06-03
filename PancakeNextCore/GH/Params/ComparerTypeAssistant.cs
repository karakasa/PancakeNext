using Grasshopper2.Data;
using Grasshopper2.Types.Assistant;
using GrasshopperIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
public sealed class ComparerTypeAssistant : TypeAssistant<ICustomComparer>
{
    public override ICustomComparer Copy(ICustomComparer instance)
    {
        return instance;
    }

    public override string DescribePrimary(Pear<ICustomComparer> pear)
    {
        return pear.Item?.ToString() ?? "<null>";
    }

    static readonly Name name = "BuiltInId";

    public ComparerTypeAssistant() : base("Pancake.ICustomComparer")
    {
    }

    public override bool Read(IReader reader, Name location, out ICustomComparer instance)
    {
        var id = reader.TryRead<int>(location, -1);
        if (id < 0 || id > 4)
        {
            instance = null;
            return false;
        }

        instance = CustomComparer.ByBuiltInId(id);
        return true;
    }

    public override bool Write(IWriter writer, Name location, ICustomComparer instance)
    {
        if (instance is CustomComparer cc)
        {
            var id = cc.BuiltinId;
            if (id >= 0)
            {
                writer.Integer32(location, id);
                return true;
            }
        }

        return false;
    }
}
