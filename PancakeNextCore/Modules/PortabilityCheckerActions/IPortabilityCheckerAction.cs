using Pancake.Modules.PortabilityChecker;
using Pancake.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pancake.Modules.PortabilityCheckerActions
{
    public interface IPortabilityCheckerAction
    {
        int Order { get; }
        bool ShouldEnable(ResultEntry[] entries);
        bool ShouldEnableOnSelection(ResultEntry entry);
        bool SelectionNotRequired { get; }
        void DoAction(IFormPortabilityReport form, ResultEntry selectedEntry);
        string ButtonText { get; }
    }
}
