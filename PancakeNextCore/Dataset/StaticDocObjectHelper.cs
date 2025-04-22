using Grasshopper2.UI;
using PancakeNextCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PancakeNextCore.Utility;
using System.Reflection;
using PancakeNextCore.Attributes;

namespace PancakeNextCore.Dataset;
internal static class StaticDocObjectHelper
{
    public static Nomen CreateNomen<T>(
#if !NET
        ref string? cachedName, ref string? cachedDesc
#endif
        ) where T : IPancakeLocalizable<T>
    {
        string name, desc;

#if NET
        name = T.StaticLocalizedName;
        desc = T.StaticLocalizedDescription;
#else
        name = (cachedName ??= (string)ReflectionHelper.GetStaticProperty<T>("StaticLocalizedName")!);
        desc = (cachedDesc ??= (string)ReflectionHelper.GetStaticProperty<T>("StaticLocalizedDescription")!);
#endif

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(desc))
        {
            throw new InvalidOperationException($"Fail to retrieve name or description for {typeof(T).Name}");
        }

        var category = typeof(T).GetCustomAttribute<ComponentCategoryAttribute>() ??
            throw new InvalidOperationException($"Fail to retrieve category for {typeof(T).Name}");

        var categoryName = ComponentLibrary.GetCategoryFriendlyName(category.SectionName);
        return new Nomen(name, desc, ComponentLibrary.PanelName, categoryName, category.SubPanelIndex, category.Rank);
    }
}
