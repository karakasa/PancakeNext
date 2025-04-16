using Grasshopper;
using Pancake.GH.Tweaks;
using Pancake.Modules.PortabilityChecker;
using Pancake.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pancake.Modules.PortabilityCheckerActions
{
    public class LocateAction : IPortabilityCheckerAction
    {
        public bool SelectionNotRequired => false;
        public string ButtonText => "Locate";

        public void DoAction(IFormPortabilityReport form, ResultEntry selectedEntry)
        {
            switch (selectedEntry.ResultType)
            {
                case ResultEntry.Type.File:
                    UiHelper.OpenFileSelected(selectedEntry.AssociatedFile);
                    break;

                case ResultEntry.Type.Object:
                    var guid = selectedEntry.AssociatedObject;
                    var doc = Instances.ActiveCanvas?.Document;

                    if (doc == null || guid == Guid.Empty)
                        return;

                    var docObj = doc.FindObject(guid, true);
                    if (docObj == null)
                    {
                        UiHelper.MinorError("Portability.NotInDocument", Strings.TheObjectIsNotInTheActiveDocumentItMayBeInACluster);
                        return;
                    }

                    GhGui.ZoomToObjects(new[] { docObj });
                    break;
            }
        }

        public bool ShouldEnable(ResultEntry[] entries)
            => true;

        public bool ShouldEnableOnSelection(ResultEntry entry)
            => entry.ResultType == ResultEntry.Type.Object || entry.ResultType == ResultEntry.Type.File;
        public int Order => 0;
    }
}
