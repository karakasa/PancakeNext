using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Pancake.GH.Tweaks;
using Pancake.Utility;
using ScriptComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pancake.Modules.PortabilityChecker
{
    public class ExternalFiles : Base
    {
        public override string Name => "External Files";
        public override string GetSectionName() => Strings.ExternalFiles;

        public override bool InternalModule => true;

        private IEnumerable<ResultEntry> AnalyzeObject(IGH_ActiveObject docObj)
        {
            switch (docObj)
            {
                case Component_AbstractScript objScript:
                    foreach (var refLocation in
                        objScript.ScriptSource.References)
                    {
                        yield return ResultEntry.FromFile(Strings.OtherReferences, refLocation);
                    }

                    break;
                case Param_FilePath objFilePath:
                    foreach (var file in objFilePath.PersistentData)
                    {
                        if (!File.Exists(file.ToString())) continue;
                        yield return ResultEntry.FromFile(Strings.FileReference, file.ToString());
                    }

                    break;
                case GH_ImageSampler sampler:
                    if (!sampler.ImageSaveInFile)
                    {
                        yield return ResultEntry.FromFile(Strings.ImageFile, sampler.ImageFilePath);
                    }

                    break;
            }

            if (docObj.GetType().Name == "DataInputComponent" &&
                docObj.GetType().Namespace == "IOComponents")
            {
                // these component are for Rhino 6 only. Since we are using SDK of Rhino 5,
                // reflection is used to extract filepath
                string filepath = null;
                try
                {
                    filepath = ReflectionHelper.GetProperty(docObj, "SourceFile").ToString();
                }
                catch (Exception e)
                {
                    // ignored
                }

                if (!string.IsNullOrEmpty(filepath) && File.Exists(filepath))
                    yield return ResultEntry.FromFile(Strings.DataFile, filepath);
            }

            if (docObj is GH_Component component)
            {
                foreach (var it2 in component.Params.Input
                    .Where(p => p is Param_FilePath && p.SourceCount == 0)
                    .SelectMany(AnalyzeObject))
                    yield return it2;
            }
        }
        public override IEnumerable<ResultEntry> AnalyzeDocument(GH_Document doc)
        {
            foreach (var it in doc.ObjectsRecursive().OfType<IGH_ActiveObject>())
                foreach (var it2 in AnalyzeObject(it))
                    yield return it2;
        }
    }
}
