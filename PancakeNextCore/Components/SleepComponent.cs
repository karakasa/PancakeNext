using Grasshopper2.Components;
using GrasshopperIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Components;
[IoId("{BF3695F0-1D90-47FF-AAD9-9E79080CB812}")]
public sealed class SleepComponent : Component
{
    public SleepComponent() : base(new Grasshopper2.UI.Nomen("Sleep", "Sleep", "Pancake", "Internal"))
    {
    }

    public SleepComponent(IReader reader) : base(reader)
    {
    }

    protected override void AddInputs(InputAdder inputs)
    {
        inputs.AddInteger("Timeout", "T", "");
        inputs.AddBoolean("Sleep", "Sl", "Sleep");
    }

    protected override void AddOutputs(OutputAdder outputs)
    {
        outputs.AddBoolean("OK", "OK", "OK");
    }

    protected override void Process(IDataAccess access)
    {
        access.GetItem(0, out int time);
        access.GetItem(1, out bool ok);

        if (!ok)
        {
            access.SetItem(0, false);
            return;
        }

        if (time < 0)
        {
            access.AddError("Wrong time", "Time must be >= 0.");
            return;
        }

        Thread.Sleep(time);
        access.SetItem(0, true);
    }
}
