using Grasshopper2.Components;
using Grasshopper2.FileTypes.ESRI;
using Grasshopper2.Parameters;
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
[IoId("1FAE7885-835B-4265-9CE1-28DE34F0703D")]
public sealed class pcChainComparer : PancakeComponent<pcChainComparer>, IPancakeLocalizable<pcChainComparer>
{
    public static string StaticLocalizedName => "Chain Comparer"; // TODO

    public static string StaticLocalizedDescription => "Chain Comparers"; // TODO
    public pcChainComparer()
    {
    }

    public pcChainComparer(IReader reader) : base(reader)
    {
    }

    protected override void RegisterInputs()
    {
        AddParam<ComparerParameter>("comparer", Access.Item);
        AddParam<ComparerParameter>("comparer", Access.Item);
    }

    protected override void RegisterOutputs()
    {
        AddParam<ComparerParameter>("comparer", Access.Item);
    }

    protected override void Process(IDataAccess access)
    {
        var list = new List<CustomComparer>();
        for (var i = 0; i < access.CountIn; i++)
        {
            if (access.GetItem(i, out ICustomComparer comparer) && comparer is CustomComparer inst)
            {
                list.Add(inst);
            }
        }

        if (list.Count == 0)
        {
            access.AddError("Wrong input", "There is no valid comparer input.");
            return;
        }

        ICustomComparer comp;
        if (list.Count == 1)
        {
            comp = list[0];
        }
        else
        {
            var col = new CustomComparerCollection();
            col.AddRange(list);
            comp = col;
        }

        access.SetItem(0, comp);
    }

    public override void DoCreateParameter(Side side, int index)
    {
        Parameters.AddInput(new ComparerParameter(Access.Item), index);
    }

    public override void DoRemoveParameter(Side side, int index)
    {
        base.DoRemoveParameter(side, index);
    }

    public override void VariableParameterMaintenance()
    {
        for (var i = 0; i < Parameters.InputCount; i++)
        {
            var str = i.ToString();
            var p = Parameters.Input(i);
            p.ModifyNameAndInfo(str, str);
            p.UserName = str;
        }
    }

    public override bool CanCreateParameter(Side side, int index)
    {
        return side is Side.Input;
    }

    public override bool CanRemoveParameter(Side side, int index)
    {
        return side is Side.Input && Parameters.InputCount > 1;
    }
}