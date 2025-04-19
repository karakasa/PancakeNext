using PancakeNextCore.GH.Tweaks;
using PancakeNextCore.PancakeMgr;

namespace PancakeNextCore.Helper;

public sealed class ParamContentHint : CanvasArtistFeature<GhParamContentArtist>
{
    public const string Name = "Param Content";
    public override string GetName() => Name;
}
