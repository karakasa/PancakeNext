using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
using PancakeNextCore.Dataset;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;
public abstract class PancakeParameter<TContent, TParameter> : Parameter<TContent>
    where TParameter : IPancakeLocalizable<TParameter>
{
    public PancakeParameter(IReader reader) : base(reader)
    {
    }

    public PancakeParameter() : this(Access.Tree)
    {
    }

    public PancakeParameter(Access access) : base(CreateNomen(), access)
    {
    }

#if NET
    private static Nomen CreateNomen()
    {
        return StaticDocObjectHelper.CreateNomen<TParameter>();
    }
#else
    private static string? CachedLocalizedName;
    private static string? CachedLocalizedDescription;
    private static Nomen CreateNomen()
    {
        return StaticDocObjectHelper.CreateNomen<TParameter>(ref CachedLocalizedName, ref CachedLocalizedDescription);
    }
#endif
}
