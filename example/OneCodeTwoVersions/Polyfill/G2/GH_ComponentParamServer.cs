using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public sealed class GH_ComponentParamServer
{
    public GH_Component Owner { get; }

    private List<IGH_Param>? _inputs = null;
    private List<IGH_Param>? _outputs = null;
    public List<IGH_Param> Input => _inputs ??= CreateInputs();
    public List<IGH_Param> Output => _outputs ??= CreateOutputs();
    internal GH_ComponentParamServer(GH_Component comp_owner)
    {
        Owner = comp_owner;
    }

    internal void InvalidateList()
    {
        _inputs = null;
        _outputs = null;
    }
    private List<IGH_Param> CreateInputs()
    {
        return [.. Owner.Parameters.Inputs.Select(ParameterWrapper.CreateFrom)];
    }
    private List<IGH_Param> CreateOutputs()
    {
        return [.. Owner.Parameters.Outputs.Select(ParameterWrapper.CreateFrom)];
    }
    public bool UnregisterParameter(IGH_Param param) => UnregisterParameter(param, true);

    public bool UnregisterParameter(IGH_Param param, bool isolate) => param.Kind switch
    {
        GH_ParamKind.input => UnregisterInputParameter(param, isolate),
        GH_ParamKind.output => UnregisterOutputParameter(param, isolate),
        _ => false,
    };

    public bool UnregisterInputParameter(IGH_Param param) => UnregisterInputParameter(param, true);

    public bool UnregisterInputParameter(IGH_Param param, bool isolate)
    {
        var ind = Owner.Parameters.IndexOfInput(param.InstanceGuid);
        if (ind < 0) return false;
        Owner.Parameters.RemoveInput(ind);
        InvalidateList();
        return true;
    }

    public bool UnregisterOutputParameter(IGH_Param param) => UnregisterOutputParameter(param, true);

    public bool UnregisterOutputParameter(IGH_Param param, bool isolate)
    {
        var ind = Owner.Parameters.IndexOfOutput(param.InstanceGuid);
        if (ind < 0) return false;
        Owner.Parameters.RemoveOutput(ind);
        InvalidateList();
        return true;
    }
    public bool RegisterInputParam(IGH_Param new_param) => RegisterInputParam(new_param, int.MaxValue);

    public bool RegisterInputParam(IGH_Param new_param, int at_index)
    {
        var ind = Owner.Parameters.IndexOfInput(new_param.InstanceGuid);
        if (ind >= 0) return true;

        Owner.Parameters.AddInput(new_param.UnderlyingObject, at_index);
        InvalidateList();
        return true;
    }

    public bool RegisterOutputParam(IGH_Param new_param) => RegisterOutputParam(new_param, int.MaxValue);

    public bool RegisterOutputParam(IGH_Param new_param, int at_index)
    {
        var ind = Owner.Parameters.IndexOfOutput(new_param.InstanceGuid);
        if (ind >= 0) return true;

        Owner.Parameters.AddOutput(new_param.UnderlyingObject, at_index);
        InvalidateList();
        return true;
    }

    public int IndexOfInputParam(string name)
    {
        checked
        {
            int num = Input.Count - 1;
            for (int i = 0; i <= num; i++)
            {
                if (Input[i].Name.Equals(name, StringComparison.Ordinal))
                {
                    return i;
                }
            }
            return -1;
        }
    }

    public int IndexOfOutputParam(string name)
    {
        checked
        {
            int num = Output.Count - 1;
            for (int i = 0; i <= num; i++)
            {
                if (Output[i].Name.Equals(name, StringComparison.Ordinal))
                {
                    return i;
                }
            }
            return -1;
        }
    }

    public int IndexOfInputParam(Guid id) => Owner.Parameters.IndexOfInput(id);

    public int IndexOfOutputParam(Guid id) => Owner.Parameters.IndexOfOutput(id);

    public IGH_Param? Find(Guid id)
    {
        int index;
        if ((index = Owner.Parameters.IndexOfInput(id)) >= 0)
        {
            return Input[index];
        }
        if ((index = Owner.Parameters.IndexOfOutput(id)) >= 0)
        {
            return Output[index];
        }
        return null;
    }
}