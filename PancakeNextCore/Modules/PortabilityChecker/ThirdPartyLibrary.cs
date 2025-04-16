using Grasshopper;
using Grasshopper.Kernel;
using Pancake.GH.Tweaks;
using Pancake.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pancake.Modules.PortabilityChecker
{
    public class ThirdPartyLibrary : Base
    {
        public override string Name => Strings.ThirdPartyLibraries;

        public override bool InternalModule => true;
        public override string GetSectionName() => Name;

        public override IEnumerable<ResultEntry> AnalyzeDocument(GH_Document doc)
        {
            var compServer = Instances.ComponentServer;
            var libIncluded = new HashSet<Guid>();
            var libCache = new Dictionary<Guid, string>();

            foreach (var it in doc.ObjectsRecursive().OfType<IGH_ActiveObject>())
            {
                var guid = it.ComponentGuid;
                if (libCache.TryGetValue(guid, out var libName))
                {
                    yield return ResultEntry.FromObject(Strings._3rdPartyComponents, it, Strings.FromThirdParty + libName);
                }
                else
                {
                    var info = compServer.FindAssemblyByObject(it);
                    if (info == null || info.IsCoreLibrary)
                        continue;

                    yield return ResultEntry.FromObject(Strings._3rdPartyComponents, it, Strings.FromThirdParty + info.Name);
                    libCache[guid] = info.Name;
                    libIncluded.Add(info.Id);
                }
            }

            foreach (var it in libIncluded)
            {
                var info = compServer.FindAssembly(it);
                yield return ResultEntry.FromDesc(Strings._3rdPartyLibraries, info.Name, info.Description);
            }

            var files = new HashSet<string>();

            foreach (var it in libIncluded)
            {
                var info = compServer.FindAssembly(it);
                if (File.Exists(info.Location))
                {
                    files.Add(info.Location);
                    DependencyWalker.WalkThroughAssemblyDependency(info.Assembly, files);
                }
            }

            foreach (var it in files)
            {
                yield return ResultEntry.FromFile(Strings.ReferencedFiles, it);
            }
        }
    }
}
