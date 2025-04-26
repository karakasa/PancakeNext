using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public interface IGH_InstanceDescription
{
    string Name { get; set; }
    string NickName { get; set; }
    string Description { get; set; }
    string Category { get; }
    string SubCategory { get; }
    Guid InstanceGuid { get; }
}
