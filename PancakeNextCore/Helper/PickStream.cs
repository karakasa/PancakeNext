using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Dataset;
using Pancake.GH.Tweaks;
using Grasshopper;
using Grasshopper.Kernel;

namespace PancakeNextCore.Helper;

class PickStream
{
    private static bool? _includeComp = null;

    public static bool IncludeComponent
    {
        get
        {
            if (_includeComp == null)
                _includeComp = Config.Read("IncludeComp", true, true);
            try
            {
                return _includeComp.Value;
            }
            catch (Exception)
            {
                return true;
            }
        }

        set
        {
            _includeComp = value;
            Config.Write("IncludeComp", _includeComp.Value.ToString());
        }
    }

    private static List<IGH_ActiveObject> GetConnectedObjs(bool upstream, bool downstream)
    {
        var ghDoc = Instances.ActiveCanvas?.Document;
        if (ghDoc == null) return null;

        var selected = (from obj in ghDoc.SelectedObjects()
                        where obj is IGH_ActiveObject
                        select obj as IGH_ActiveObject).ToList();
        ghDoc.DeselectAll();
        var streamedObj = GhStream.GetConnectedObjects(selected, upstream, downstream);
        streamedObj.AddRange(selected);
        return streamedObj;
    }

    public static void PickStreamObjs(bool upstream, bool downstream)
    {
        var streamedObj = GetConnectedObjs(upstream, downstream);
        var zoomObjs = new List<IGH_DocumentObject>();
        foreach (var obj in streamedObj)
        {
            obj.Attributes.Selected = true;
            zoomObjs.Add(obj);
        }

        GhGui.ZoomToObjects(zoomObjs);
        Instances.RedrawCanvas();
    }

    public static void PickParamAtFarEnd(bool upstream, bool downstream)
    {
        var zoomObjs = new List<IGH_DocumentObject>();
        var streamedObj = GetConnectedObjs(upstream, downstream);
        foreach (var obj in streamedObj)
        {
            switch (obj)
            {
                case IGH_Param param when param.Kind == GH_ParamKind.floating:
                    if (upstream && param.SourceCount == 0 || downstream && param.Recipients.Count == 0)
                    {
                        obj.Attributes.Selected = true;
                        zoomObjs.Add(obj);
                    }
                    break;
                case IGH_Component comp when IncludeComponent:
                    if (downstream)
                    {
                        if (comp.Params.Output.Count == 0 || comp.Params.Output.Sum(x => x.Recipients.Count) == 0)
                        {
                            comp.Attributes.Selected = true;
                            zoomObjs.Add(obj);
                        }
                    }

                    if (upstream)
                    {
                        if (comp.Params.Input.Any(x => x.SourceCount == 0 && InternalizeGeo.ContainGeometryReference(x)))
                        {
                            comp.Attributes.Selected = true;
                            zoomObjs.Add(obj);
                        }
                    }
                    break;
            }
        }

        GhGui.ZoomToObjects(zoomObjs);
        Instances.RedrawCanvas();
    }
}
