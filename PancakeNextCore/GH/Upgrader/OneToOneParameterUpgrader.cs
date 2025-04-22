using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Interop;
using Grasshopper2.Parameters;
using GrasshopperIO;
using PancakeNextCore.Components.IO;
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
public abstract class OneToOneParameterUpgrader<T> : IUpgradeGh1Parameter where T : IParameter, new()
{
    public virtual Guid Grasshopper1Id { get; } = (typeof(T).GetCustomAttribute<IoIdAttribute>() ?? throw new ArgumentException("Component must have an IoIdAttribute")).Id;
    public IDocumentObject Upgrade(IGH_Param component)
    {
        var comp = new T();
        component.Transfer(comp);
        return comp;
    }
}
