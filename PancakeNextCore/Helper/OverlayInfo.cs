using Grasshopper;
using Pancake.GH.Tweaks;
using PancakeNextCore.PancakeMgr;

namespace PancakeNextCore.Helper;

public class OverlayInfo : Feature
{
    public const string Name = "Overlay Info";
    private static void RedrawCanvas()
    {
        Instances.RedrawCanvas();
    }

    public override FeatureManager.LoadStage GetExpectedLoadStage() => FeatureManager.LoadStage.UI;

    public override string GetName() => Name;

    public override bool IsEffective() => GhParamAccessArtist.Instance.Enabled;

    public override void OnLoad()
    {
        GhParamAccessArtist.Instance.Enabled = true;
        RedrawCanvas();
    }

    public override void OnUnload()
    {
        GhParamAccessArtist.Instance.Enabled = false;
        RedrawCanvas();
    }

    public bool HintOptional
    {
        get => GhParamAccessArtist.Instance.HintOptional;
        set
        {
            GhParamAccessArtist.Instance.HintOptional = value;
            RedrawCanvas();
        }
    }

    public override bool IsDefaultEnabled() => false;
}
