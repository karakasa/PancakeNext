using Eto.Drawing;
using Grasshopper2.UI.Icon;

namespace PancakeNextCore.Utility.PathBasedIcon;

/// <summary>
/// A pixel icon of 4 images, one for each size.
/// </summary>
internal sealed class PixelIconPlayingMahjong : AbstractIcon
{
    private readonly Bitmap[] _images;
    public PixelIconPlayingMahjong(Bitmap[] images) : base(IconType.Pixel)
    {
        if (images.Length != 4) throw new ArgumentException("Images must be length of 4.");
        _images = images;
    }

    private static int FindBestMatch(int size)
    {
        return size switch
        {
            <= (IconHost.Size0 + IconHost.Size1) / 2 => 0,
            <= (IconHost.Size1 + IconHost.Size2) / 2 => 1,
            <= (IconHost.Size2 + IconHost.Size3) / 2 => 2,
            _ => 3
        };
    }
    // static readonly Font _font = new Font(FontFamilies.Sans, 8);
    protected override void DrawInternal(IconContext context)
    {
        var onScreenSize = context.Frame.Width * context.Zoom;
        var index = FindBestMatch((int)onScreenSize);
        context.Context.Graphics.DrawImage(_images[index], context.Frame);
        // context.Context.Graphics.DrawText(_font, Color.FromArgb(0,0,0), context.Frame.Center, $"{index}");
    }
}
