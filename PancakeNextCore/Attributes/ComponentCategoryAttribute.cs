using Grasshopper2.UI;
using System;

namespace PancakeNextCore.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ComponentCategoryAttribute(string section, int subPanelIndex = 0, Rank rank = Rank.Normal) : Attribute
{
    public string SectionName { get; private set; } = section;
    public int SubPanelIndex { get; private set; } = subPanelIndex;
    public Rank Rank { get; private set; } = rank;
}