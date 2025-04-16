using Grasshopper.Kernel;
using Pancake.Dataset;
using Pancake.GH.Tweaks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pancake.Modules.PortabilityChecker
{
    public abstract class ComponentIdChecker : Base
    {
        public override string Name => GetType().Name;

        private HashSet<Guid> _internalGuids = new HashSet<Guid>();
        protected virtual HashSet<Guid> Guids => _internalGuids;
        public override IEnumerable<ResultEntry> AnalyzeDocument(GH_Document doc)
        {
            return doc.ObjectsRecursive()
                .Where(FilterObject)
                .Select(docObj => ResultEntry.FromObject(GetSectionName(), docObj));
        }

        protected bool FilterObject(IGH_DocumentObject docObj)
        {
            if (docObj is null) return false;

            return _internalGuids.Contains(docObj.ComponentGuid)
                || R7OnlyComponent.IsR7Only(docObj.ComponentGuid);
        }
    }

    internal class Rh5WinTargetChecker : ComponentIdChecker
    {
        public override bool InternalModule => true;

        public override string GetSectionName() => string.Format(Strings.ComponentsUnavailableIn0, "Rhino 5 Windows");

        public Rh5WinTargetChecker()
        {
            Guids.UnionWith(R6OnlyComponent.GuidList);
            Guids.UnionWith(R6OnlyComponent.GuidListMac);
        }
    }

    internal class Rh5MacTargetChecker : ComponentIdChecker
    {
        public override bool InternalModule => true;

        public override string GetSectionName() => string.Format(Strings.ComponentsUnavailableIn0, "Rhino 5 Mac");

        public Rh5MacTargetChecker()
        {
            Guids.UnionWith(R6OnlyComponent.GuidList);
        }
    }

    internal class Rh6TargetChecker : ComponentIdChecker
    {
        public override bool InternalModule => true;

        public override string GetSectionName() => string.Format(Strings.ComponentsUnavailableIn0, "Rhino 6");

        public Rh6TargetChecker()
        {
        }
    }
}
