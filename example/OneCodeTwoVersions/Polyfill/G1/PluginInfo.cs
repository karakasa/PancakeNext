using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public abstract class PluginInfo(Guid id, string name, string desc, Version version) : GH_AssemblyInfo
{
    private readonly Guid _id = id;
    private readonly string _name = name;
    private readonly string _desc = desc;
    private readonly Version _version = version;
    public override sealed Bitmap Icon => IconInternal;
    public virtual System.Drawing.Bitmap? IconInternal => null;
    public override sealed string Name => _name;
    public override sealed string Version => _version.ToString();
    public override sealed string Description => _desc;
    public override GH_LibraryLicense License => GH_LibraryLicense.unset;
    public override string AuthorName => string.Empty;
    public override string AuthorContact => string.Empty;
    public override sealed Guid Id => _id;
}