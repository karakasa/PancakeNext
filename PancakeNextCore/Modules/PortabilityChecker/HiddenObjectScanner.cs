using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Pancake.GH.Tweaks;
using Pancake.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pancake.Modules.PortabilityChecker
{
    public class HiddenObjectScanner : Base
    {
        public override string Name => nameof(HiddenObjectScanner);

        public override bool InternalModule => true;

        private struct HiddenIssue
        {
            public IGH_DocumentObject DocObject;
            public HiddenIssueReason Reason;
        }
        [Flags]
        private enum HiddenIssueReason
        {
            InvalidBounds = 1,
            CustomAttribute = 2,
            TooSmallScribble = 4,
            MissingComponent = 8
        }
        public override string GetSectionName() => "Hidden objects";

        private const double ZeroTolerance = 0.01;
        public override IEnumerable<ResultEntry> AnalyzeDocument(GH_Document doc)
        {
            HiddenIssue issue = default;

            foreach (var it in doc.ObjectsRecursive())
            {
                bool result = false;

                try
                {
                    result = ScanForHiddenIssues(it, ref issue);
                }
                catch
                {
                }

                if (!result)
                    continue;

                switch (issue.Reason)
                {
                    case HiddenIssueReason.InvalidBounds:
                        yield return ResultEntry.FromObject(GetSectionName(), issue.DocObject, "Invalid bounds. The component may be a hidden component to store invisible information.");
                        break;
                    case HiddenIssueReason.TooSmallScribble:
                        yield return ResultEntry.FromObject(GetSectionName(), issue.DocObject, "Scribble too small. The component may be a hidden component to store invisible information.");
                        break;
                }
            }
        }

        public IEnumerable<IGH_DocumentObject> GetHiddenObjectList(GH_Document doc)
        {
            HiddenIssue issue = default;

            foreach (var it in doc.ObjectsRecursive())
            {
                var result = false;

                try
                {
                    result = ScanForHiddenIssues(it, ref issue);
                }
                catch
                {
                }

                if (!result)
                    continue;

                yield return issue.DocObject;
            }
        }

        private static bool ScanForHiddenIssues(IGH_DocumentObject it, ref HiddenIssue issue)
        {
            if (it is GH_Group)
                return false;

            if (Math.Abs(it.Attributes.Bounds.Height) < ZeroTolerance
                || Math.Abs(it.Attributes.Bounds.Width) < ZeroTolerance
                )
            {
                issue.DocObject = it;
                issue.Reason = HiddenIssueReason.InvalidBounds;

                return true;
            }

            if (it is GH_Scribble scribble)
            {
                if (scribble.Font.Size < 1.0 && !VersionTrack.IsPancakeWatermarkObject(scribble))
                {
                    issue.DocObject = it;
                    issue.Reason = HiddenIssueReason.TooSmallScribble;
                    return true;
                }
            }

            return false;
        }
    }
}
