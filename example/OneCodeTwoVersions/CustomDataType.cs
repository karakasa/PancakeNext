using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions;
public sealed class CustomDataType
{
    public int TestData1 { get; set; }
    public int TestData2 { get; set; }
    public override string ToString() => $"{TestData1},{TestData2}";
    public CustomDataType Clone()
    {
        return (CustomDataType)MemberwiseClone();
    }
}
