using Grasshopper2.Framework;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract class PluginInfo(Guid id, string name, string desc, Version version) : Plugin(id, new Nomen(name, desc), version)
{
    public override sealed IIcon Icon
    {
        get
        {
            var icon = IconInternal;
            if (icon is null) return null!;
            return AbstractIcon.FromBitmap(icon.ToEto());
        }
    }
    public virtual System.Drawing.Bitmap? IconInternal => null;
    public string Name => Nomen.Name;
    public new string Version => base.Version.ToString();
    public string Description => Nomen.Info;
    public virtual GH_LibraryLicense License => GH_LibraryLicense.unset;
    public virtual string AuthorName => string.Empty;
    public virtual string AuthorContact => string.Empty;
    public override sealed string Author => AuthorName;
    public override string LicenceDescription => LicenseToString(License);
    public override sealed string Contact => AuthorContact;

    internal static string LicenseToString(GH_LibraryLicense license) => license switch
    {
        GH_LibraryLicense.alpha or GH_LibraryLicense.beta or GH_LibraryLicense.free or
        GH_LibraryLicense.trial or GH_LibraryLicense.commercial or GH_LibraryLicense.educational or
        GH_LibraryLicense.developer or GH_LibraryLicense.reseller => license.ToString(),
        _ => "",
    };
}