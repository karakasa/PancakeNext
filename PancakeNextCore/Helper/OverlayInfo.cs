using PancakeNextCore.GH.Tweaks;
using PancakeNextCore.PancakeMgr;

namespace PancakeNextCore.Helper;

public sealed class OverlayInfo : CanvasArtistFeature<GhParamAccessArtist>
{
    public const string Name = "Overlay Info";
    public override string GetName() => Name;
    public static bool HintOptional
    {
        get => GhParamAccessArtist.HintOptional;
        set
        {
            GhParamAccessArtist.HintOptional = value;
            RedrawCanvas();
        }
    }
}
