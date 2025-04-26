using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill.G2;
public interface IGH_TypeHint
{
    string TypeName { get; }
    Guid HintID { get; }
    bool Cast(object data, out object target);
}
