using Grasshopper2.UI;
using PancakeNextCore.GH.Tweaks;
using PancakeNextCore.PancakeMgr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.Helper;
public abstract class CanvasArtistFeature<T> : Feature where T : ICanvasArtist, new()
{
    private bool _enabled = false;
    private readonly T _artist = new();
    public override FeatureManager.LoadStage GetExpectedLoadStage() => FeatureManager.LoadStage.UI;

    public override string GetName() => _artist.Name;

    public override bool IsEffective()
    {
        return _enabled;
    }

    public override void OnLoad()
    {
        var canvas = Editor.Instance?.Canvas;
        if (canvas is not null)
        {
            _artist.Register(canvas);
            _enabled = true;
            RedrawCanvas();
        }
    }

    public override void OnUnload()
    {
        var canvas = Editor.Instance?.Canvas;
        if (canvas is not null)
        {
            _artist.Unregister(canvas);
            _enabled = false;
            RedrawCanvas();
        }
    }
    public override bool IsDefaultEnabled() => false;

    protected static void RedrawCanvas()
    {
        Editor.Instance?.Canvas?.Invalidate();
    }
}
