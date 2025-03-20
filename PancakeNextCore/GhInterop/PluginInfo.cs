using Grasshopper2.Framework;
using Grasshopper2.UI.Icon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GhInterop;

public sealed class PluginInfo : Plugin
{
    public static readonly Guid PluginId = new("{4B8EE19F-878C-4B9A-BFF3-DD5CC938B9B3}");
    public static readonly Version CoreVersion = typeof(PluginInfo).Assembly.GetName().Version;
    public PluginInfo()
        : base(PluginId,
              new("Pancake", "Provides tools for teamworking"),
              CoreVersion)
    {
        var bitmap = Eto.Drawing.Bitmap.FromResource("PancakeNext.Resources.Pancake48.png");
        _icon = AbstractIcon.FromBitmap(bitmap);

        PluginLifetime.PriorityLoad();
    }
    private IIcon _icon;
    public override string Author => "Keyu Gan";
    public override IIcon Icon => _icon;
    public override string LicenceDescription => base.LicenceDescription;
    public override string LicenceAgreement => "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.";
}
