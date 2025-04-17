using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using PancakeNextCore.Utility;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Helper;
internal sealed class OrphanedObjectLocator(RhinoDoc rhinoDoc)
{
    public readonly struct Record(IParameter param)
    {
        internal bool IsEmpty => Parameter is null;
        public readonly IParameter? Parameter { get; } = param;
        public List<Guid> Guids { get; } = [];
    }

    public List<Record> Results { get; } = [];
    private readonly ParameterizedCache<RhinoDoc, Guid, bool> _docIdCache = new(rhinoDoc, DoesIdExistInRhinoDoc);
    private static bool DoesIdExistInRhinoDoc(RhinoDoc doc, Guid guid) => doc.Objects.FindId(guid) is not null;
    public void Analyze(Document doc)
    {
        foreach (var p in doc.AllParameters(true))
        {
            FindInParam(p);
        }
    }

    private void FindInParam(IParameter parameter)
    {
        if (parameter.Inputs.Count > 0) return;

        Record currentList = default;

        foreach (var pear in parameter.PersistentDataWeak.AllPears)
        {
            if (!pear.TryGetReferenceId(out var guid) || // Not a reference
                _docIdCache[guid]) // Found in RhinoDoc
                continue;

            if (currentList.IsEmpty)
                Results.Add(currentList = new(parameter));

            currentList.Guids.Add(guid);
        }
    }
}