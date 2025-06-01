using Grasshopper.Kernel.Types;
using OneCodeTwoVersions.Polyfill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions;
public sealed class CustomDataTypeGoo : GH_Goo<CustomDataType>
{
    public override bool IsValid => Value is not null;

    public override string TypeName => "Custom data type";

    public override string TypeDescription => "A lightweight goo wrapper around custom data type";

    public override IGH_Goo Duplicate()
    {
        return new CustomDataTypeGoo { Value = Value };
    }

    public override string ToString()
    {
        return Value?.ToString();
    }
    public override bool CastFrom(object source)
    {
        if (source is CustomDataType v)
        {
            Value = v;
            return true;
        }

        return base.CastFrom(source);
    }
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q) == typeof(CustomDataType))
        {
            target = (Q)(object)Value;
            return true;
        }

        return base.CastTo(ref target);
    }
}
