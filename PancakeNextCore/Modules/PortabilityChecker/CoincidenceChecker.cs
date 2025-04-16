using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Pancake.Utility;

namespace Pancake.Modules.PortabilityChecker
{
    public class CoincidenceChecker : Base
    {
        public override string Name => "CoincidenceChecker";
        public override bool InternalModule => true;
        public override string GetSectionName() => Strings.FollowingPairsAreCoincidentOnTheCanvas;

        public override IEnumerable<ResultEntry> AnalyzeDocument(GH_Document doc)
        {
            return CheckCoincidentObjects(doc.Objects);
        }

        internal IEnumerable<ResultEntry> CheckCoincidentObjects(
            ICollection<IGH_DocumentObject> objects)
        {
            var objsComponent = new List<IGH_Component>(objects.OfType<IGH_Component>());

            // the coincidence check happens in one same type of component only
            foreach (var componentOfSameType in objsComponent.GroupBy(c => c.GetType().ToString()))
            {
                var orderedComponents = componentOfSameType.OrderBy(c => c.Params.Input.Count)
                    .ThenBy(c => c.Params.Output.Count)
                    .ThenBy(c => c.Attributes.Pivot.X)
                    .ThenBy(c => c.Attributes.Pivot.Y)
                    .ToList();

                for (var i = 0; i < orderedComponents.Count - 1; i++)
                {
                    if (CheckCoincident(orderedComponents[i], orderedComponents[i + 1]))
                    {
                        yield return ResultEntry.FromObject(GetSectionName(), orderedComponents[i], $"Overlap with {orderedComponents[i + 1].Name}");
                    }
                }
            }

            var objsParams = new List<IGH_Param>(objects.OfType<IGH_Param>());

            // the coincidence check happens in one same type of param only
            foreach (var paramOfSameType in objsParams.GroupBy(c => c.GetType().ToString()))
            {
                var orderedParams = paramOfSameType.OrderBy(c => c.Attributes.Pivot.X)
                    .ThenBy(c => c.Attributes.Pivot.Y)
                    .ToList();

                for (var i = 0; i < orderedParams.Count - 1; i++)
                {
                    if (CheckCoincident(orderedParams[i], orderedParams[i + 1]))
                    {
                        yield return ResultEntry.FromObject(GetSectionName(), orderedParams[i], $"Overlap with {orderedParams[i + 1].Name}");
                    }
                }
            }
        }

        private static bool CheckCoincident(IGH_Component a, IGH_Component b)
        {
            if (a.GetType() != b.GetType())
                return false;
            if (a.Params.Input.Count != b.Params.Input.Count)
                return false;
            if (a.Params.Output.Count != b.Params.Output.Count)
                return false;

            return RhinoUtility.TestCoincidentPointF(a.Attributes.Pivot, b.Attributes.Pivot);
        }

        private static bool CheckCoincident(IGH_Param a, IGH_Param b)
        {
            if (a.GetType() != b.GetType())
                return false;

            return RhinoUtility.TestCoincidentPointF(a.Attributes.Pivot, b.Attributes.Pivot);
        }
    }
}
