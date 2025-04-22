using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Interfaces;

namespace PancakeNextCore.Components.Miscellaneous;

[ComponentCategory("logic", 0)]
[IoId("{9CA18FF9-94B8-403B-A25C-39098ECA0EB9}")]
public sealed class pcAbort : PancakeComponent<pcAbort>, IPancakeLocalizable<pcAbort>
{
    public pcAbort() { }
    public pcAbort(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.Abort;
    public static string StaticLocalizedDescription => Strings.AbortTheComputationIfTheFirstInputIsFalse;

    protected override void RegisterInputs()
    {
        AddParam<BooleanParameter>("condition");
        AddParam<GenericParameter>("passthrough", Access.Tree, Requirement.MayBeMissing);
    }

    protected override void RegisterOutputs()
    {
        AddParam<GenericParameter>("passthrough", Access.Tree);
    }

    protected override void Process(IDataAccess access)
    {
        if (access.GetItem(0, out bool condition) && condition)
        {
            Document?.Solution?.Stop();
            return;
        }

        if (access.GetITree(1, out var tree))
            access.SetTree(0, tree);
    }
}
