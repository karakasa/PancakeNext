using Grasshopper2.Doc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract class ActiveObjectWrapper<T>(T val) : IGH_ActiveObject
    where T : IDocumentObject
{
    protected T _value { get; set; } = val;
    public string Name
    {
        get => _value.Nomen.Name;
        set => _value.ModifyNameAndInfo(value, null);
    }
    public string NickName
    {
        get => _value.UserName;
        set => _value.UserName = value;
    }
    public string Description
    {
        get => _value.Nomen.Info;
        set => _value.ModifyNameAndInfo(null, value);
    }
    public string Category => _value.Nomen.Chapter;
    public string SubCategory => _value.Nomen.Section;
    public Guid InstanceGuid => _value.InstanceId;

    public bool Locked
    {
        get => _value.Activity == ObjectActivity.Disabled;
        set => _value.Activity = value ? ObjectActivity.Disabled : ObjectActivity.Enabled;
    }

    public Guid ComponentGuid => ComponentIdCacher.Instance[_value.GetType()];

    public GH_SolutionPhase Phase => _value.Phase.To1();
}
