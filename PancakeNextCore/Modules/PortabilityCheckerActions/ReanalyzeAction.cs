using Pancake.Helper;
using Pancake.Modules.PortabilityChecker;
using Pancake.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pancake.Modules.PortabilityCheckerActions
{
    public class ReanalyzeAction : IPortabilityCheckerAction
    {
        public bool SelectionNotRequired => true;
        public string ButtonText => "Re-analyze";

        public void DoAction(IFormPortabilityReport form, ResultEntry selectedEntry)
        {
            form.RefreshContent();
        }

        public bool ShouldEnable(ResultEntry[] entries)
            => true;

        public bool ShouldEnableOnSelection(ResultEntry entry)
            => true;
        public int Order => 999;
    }
}
