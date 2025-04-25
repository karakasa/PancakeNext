using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public interface IGH_Goo
{
	bool IsValid { get; }
    string IsValidWhyNot { get; }
    string TypeName { get; }

    string TypeDescription { get; }
    IGH_Goo Duplicate();
    new string ToString();
    bool CastFrom(object source);
    bool CastTo<T>(out T target);

    object ScriptVariable();
}
