using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using System;

namespace PancakeNextCore.Components.Miscellaneous;

[ComponentCategory("misc", 1)]

[IoId("2d223e87-a1ff-4a8e-85df-c0f5db0bee0a")]
public sealed class pcWaitUntil : PancakeComponent<pcWaitUntil>, IPancakeLocalizable<pcWaitUntil>
{
    public pcWaitUntil() { }
    public pcWaitUntil(IReader reader) : base(reader) { }
    public static string StaticLocalizedName => Strings.WaitUntil;
    public static string StaticLocalizedDescription => Strings.PostponeDataUntilSignalIsReceived;
    protected override void RegisterInputs()
    {
        AddParam("signal", Access.Tree);
        AddParam("data", Access.Tree);
    }
    protected override void RegisterOutputs()
    {
        AddParam("data3", Access.Tree);
    }
    protected override void Process(IDataAccess access)
    {
        access.GetITree(1, out var tree);
        access.SetTree(0, tree);
    }
    protected override IIcon? IconInternal => IconHost.CreateFromPathResource("WaitUntil");
}