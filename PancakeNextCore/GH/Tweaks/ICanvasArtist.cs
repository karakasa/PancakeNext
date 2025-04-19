using Grasshopper2.UI.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Tweaks;
public interface ICanvasArtist
{
    string Name { get; }
    void Register(Canvas canvas);
    void Unregister(Canvas canvas);
}
