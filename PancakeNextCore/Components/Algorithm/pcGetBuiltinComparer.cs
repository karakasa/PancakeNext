using Grasshopper2.Components;
using Grasshopper2.UI;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.GH.Params;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Components.Algorithm;

[ComponentCategory("misc", 0)]
[IoId("1FAE7885-835B-4265-9CE1-28DE34F0703C")]
public sealed class pcGetBuiltinComparer : PancakeComponent<pcGetBuiltinComparer>, IPancakeLocalizable<pcGetBuiltinComparer>
{
    public static string StaticLocalizedName => "Get Built-in Comparer"; // TODO

    public static string StaticLocalizedDescription => "Get Built-in Comparers"; // TODO

    private enum BuiltInComparerType : int
    {
        Default = ComparerType.Default,
        NaturalSort = ComparerType.BuiltinNaturalSort
    }
    public pcGetBuiltinComparer()
    {
    }

    public pcGetBuiltinComparer(IReader reader) : base(reader)
    {
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out int comparerType);
        var actualComparerType = comparerType switch
        {
            (int)BuiltInComparerType.Default => ComparerType.Default,
            (int)BuiltInComparerType.NaturalSort => ComparerType.BuiltinNaturalSort,
            _ => throw new ArgumentOutOfRangeException(nameof(comparerType), "Invalid comparer type.")
        };

        access.GetItem(1, out bool reversed);
        var comparer = CustomComparer.ByBuiltIn(actualComparerType, reversed);
        access.SetItem(0, comparer);
    }

    protected override void RegisterInputs()
    {
        var ip = AddParam("builtInCompare", 0);
        ip.Presets.Add("Default", "The default Pancake order, similar to Grasshopper built-in.", (int)BuiltInComparerType.Default);
        ip.Presets.Add("Natural", "Natural sort order of texts. Key must be text or will be converted to text.", (int)BuiltInComparerType.NaturalSort);

        AddParam("reversed", false);
    }

    protected override void RegisterOutputs()
    {
        AddParam<ComparerPin>("comparer");
    }
}
