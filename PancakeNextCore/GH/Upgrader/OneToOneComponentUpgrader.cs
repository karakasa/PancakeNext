using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Interop;
using GrasshopperIO;
using PancakeNextCore.Components.Io;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Upgrader;
/// <summary>
/// Provides an one-to-one component upgrader. They must have same ComponentGuid/IoId, same inputs & outputs.
/// Support of variable components is to be done.
/// </summary>
/// <typeparam name="T">Type of the new component</typeparam>
public abstract class OneToOneComponentUpgrader<T> : IUpgradeGh1Component where T : Component, new()
{
    public Guid Grasshopper1Id { get; } = (typeof(T).GetCustomAttribute<IoIdAttribute>() ?? throw new ArgumentException("Component must have an IoIdAttribute")).Id;
    public IDocumentObject Upgrade(IGH_Component component)
    {
        var comp = new T();

        EnsureMapArray(component.InputCount, component.OutputCount);

        component.TransferObjectName(comp);
        component.TransferObjectState(comp);
        component.TransferInstanceId(comp);
        component.TransferInputs(comp, InputMap);
        component.TransferOutputs(comp, OutputMap);

        return comp;
    }

    private (int, int)[]? InputMap;
    private (int, int)[]? OutputMap;

    [MemberNotNull(nameof(InputMap), nameof(OutputMap))]
    private void EnsureMapArray(int inCount, int outCount)
    {
        if (InputMap is not null && OutputMap is not null) return;

        InputMap = CreateMapArray(inCount);
        OutputMap = inCount == outCount ? InputMap : CreateMapArray(outCount);
    }

    private static (int, int)[] CreateMapArray(int count)
    {
        if (count == 0) return [];

        var arr = new (int, int)[count];
        for (var i = 0; i < arr.Length; i++)
            arr[i] = (i, i);
        return arr;
    }
}
