namespace PancakeNextCore.PancakeMgr;

public abstract class Feature
{
    public abstract void OnLoad();
    public abstract void OnUnload();
    public abstract bool IsEffective();
    public abstract string GetName();
    public abstract FeatureManager.LoadStage GetExpectedLoadStage();

    public virtual bool IsDefaultEnabled() => false;
    public virtual bool RequireRestart(bool isOnLoad) => false;
}
