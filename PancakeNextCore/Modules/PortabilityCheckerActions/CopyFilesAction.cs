using Pancake.Modules.PortabilityChecker;
using Pancake.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pancake.Modules.PortabilityCheckerActions
{
    public class CopyFilesAction : IPortabilityCheckerAction
    {
        public string ButtonText => "Copy files to..";

        public void DoAction(IFormPortabilityReport form, ResultEntry selectedEntry)
        {
            var fldr = UiHelper.FolderBrowserDialog(Strings.SelectAFolderToCopyReferencedFiles);
            if (string.IsNullOrEmpty(fldr)) return;

            try
            {
                foreach (var it in form.Results)
                {
                    if (it.ResultType != ResultEntry.Type.File) continue;
                    var newPath = Path.Combine(fldr, Path.GetFileName(it.AssociatedFile));
                    File.Copy(it.AssociatedFile, newPath, true);
                }
            }
            catch
            {
                UiHelper.MinorError("Portability.CopyFiles", Strings.CannotCopyFilesPleaseEnsureTheFolderIsWritable);
            }
        }

        public bool ShouldEnable(ResultEntry[] entries)
            => entries.Any(e => e.ResultType == ResultEntry.Type.File);

        public bool ShouldEnableOnSelection(ResultEntry entry)
            => true;
        public int Order => 1;

        public bool SelectionNotRequired => true;
    }
}
