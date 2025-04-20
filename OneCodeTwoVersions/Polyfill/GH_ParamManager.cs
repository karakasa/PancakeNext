using Grasshopper2.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract class GH_ParamManager
{
    /// <summary>
    /// Gets the total number of parameters.
    /// </summary>
    public abstract int ParamCount { get; }

    /// <summary>
    /// Gets the parameter at the given index.
    /// </summary>
    public abstract IGH_Param this[int index] { get; }

    /// <summary>
    /// Assign some common fields to a parameter object.
    /// </summary>
    protected void FixUpParameter(IGH_Param param, string name, string nickName, string description)
    {
        if (param == null)
        {
            Tracing.Assert(new Guid("{976FD532-DAF5-4a39-8E8F-221583D6C9AF}"), "param is a null reference.");
        }
        if (param.Attributes == null)
        {
            param.Attributes = new GH_LinkedParamAttributes(param, m_owner.Attributes);
        }
        param.Name = name;
        if (string.IsNullOrEmpty(nickName))
        {
            nickName = name;
        }
        param.NickName = nickName;
        if (description == null)
        {
            description = "";
        }
        if (description.Length == 0)
        {
            description = name;
        }
        param.Description = description;
    }

    /// <summary>
    /// Hide a specific parameter. If the parameter at the given index implements IGH_PreviewObject 
    /// then the Hidden flag will be set to True. Otherwise, nothing will happen.
    /// </summary>
    /// <param name="index">Index of parameter to hide.</param>
    public abstract void HideParameter(int index);
}
