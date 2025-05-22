using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using System;
using System.IO;
using System.Text;

namespace PancakeNextCore.Components.IO;

[IoId("04042ba4-ffff-42d6-a264-2eb6e701c116")]
[ComponentCategory("io", 1)]
public sealed class pcExportTXT : PancakeComponent<pcExportTXT>, IPancakeLocalizable<pcExportTXT>
{
    public static string StaticLocalizedName => Strings.ExportTXT;

    public static string StaticLocalizedDescription => Strings.ThisComponentExportsTextToAFile;

    public pcExportTXT() { }
    public pcExportTXT(IReader reader) : base(reader) { }
    protected override void RegisterInputs()
    {
        AddParam<TextParameter>("content");
        AddParam<TextParameter>("filelocation");
        AddParam("overwrite", false);
        AddParam("encoding2", "utf-8");
        AddParam("export", false);
    }
    protected override void RegisterOutputs()
    {
        AddParam<BooleanParameter>("ok");
    }

    protected override void Process(IDataAccess access)
    {
        FailInUntrusted();

        access.GetItem(0, out string content);
        access.GetItem(1, out string file);
        access.GetItem(2, out bool overwrite);
        access.GetItem(3, out string encoding);
        access.GetItem(4, out bool run);

        if (!run)
            return;

        encoding ??= "utf-8";

        access.SetItem(0, false);

        if (!overwrite && File.Exists(file))
        {
            access.AddError("Wrong destination", Strings.TargetFileExistsAndOverwriteIsFalse);
            return;
        }

        if (string.IsNullOrEmpty(file))
        {
            access.AddError("Wrong destination", Strings.PathCannotBeEmpty);
            return;
        }

        if (!Directory.Exists(System.IO.Path.GetDirectoryName(file)))
        {
            access.AddError("Wrong destination", Strings.TheDestinationDirectoryDoesnTExist);
            return;
        }

        try
        {
            Encoding? encoder = null;
            try
            {
                encoder = encoding == "default" ? Encoding.Default : Encoding.GetEncoding(encoding);
            }
            catch (ArgumentException)
            {
                access.AddError("Wrong encoding", string.Format(Strings.CannotFindTheEncoding0, encoding));
            }

            if (encoder != null)
            {
                File.WriteAllText(file, content, encoder);
                access.SetItem(0, true);
            }

        }
        catch (Exception ex)
        {
            access.AddError("IO error", ex.Message);
        }
    }

    internal static string IconResourceName => GlobalizationResolver.IsChineseCharacterSpecialized ? "ExportTXTChn" : "ExportTXT";
    protected override IIcon? IconInternal => IconHost.CreateFromPathResource(pcExportTXT.IconResourceName);
}