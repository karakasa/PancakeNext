using Pancake.GH.Tweaks;
using PancakeNextCore.PancakeMgr;

namespace PancakeNextCore.Helper;

public class ParamContentHint : Feature
{
    public override FeatureManager.LoadStage GetExpectedLoadStage() => FeatureManager.LoadStage.UI;

    public const string Name = "Param Content";
    public override string GetName() => Name;

    public override bool IsEffective() => GhParamContentArtist.Enabled;

    public override void OnLoad()
    {
        GhParamContentArtist.Enable();
    }

    public override void OnUnload()
    {
        GhParamContentArtist.Disable();
    }
}
