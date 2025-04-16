using System.Collections.Generic;
using Grasshopper.Kernel;
using Pancake.Interfaces;

namespace Pancake.Modules.PortabilityChecker
{
    public abstract class Base : IModule
    {
        public const string ObjectiveIdentifier = "PortabilityChecker";

        public abstract string GetSectionName();
        public abstract IEnumerable<ResultEntry> AnalyzeDocument(GH_Document doc);

        public abstract string Name { get; }
        public abstract bool InternalModule { get; }
        public string Objective => ObjectiveIdentifier;
    }
}
