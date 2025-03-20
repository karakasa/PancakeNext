using System;

namespace PancakeNextCore.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ComponentCategoryAttribute : Attribute
{
    public string SectionName { get; private set; } = "";
    public int SubPanelIndex { get; private set; } = -1;

    public ComponentCategoryAttribute(
        string section, int subPanelIndex)
    {
        SectionName = section;
        SubPanelIndex = subPanelIndex;
    }
}
