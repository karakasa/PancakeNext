using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.UI;
using GrasshopperIO;
using PancakeNextCore.Dataset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Components;
[IoId("{4307EB3F-8281-4EB8-BE9F-9492AF9BD742}")]
public sealed class NotAccessedParameters : Component
{
    public NotAccessedParameters() : base(new Nomen("NAP", "NAP", "Pancake", "Internal")) { }
    public NotAccessedParameters(IReader reader) : base(reader) { }
    protected override void AddInputs(InputAdder inputs)
    {
    }

    protected override void AddOutputs(OutputAdder outputs)
    {
        outputs.AddText("N", "N", "N", Grasshopper2.Parameters.Access.Twig);
    }

    protected override void Process(IDataAccess access)
    {
        access.SetTwig(0, Garden.TwigFromList(ComponentLibrary.ParametersNotAccessed()));
    }
}
