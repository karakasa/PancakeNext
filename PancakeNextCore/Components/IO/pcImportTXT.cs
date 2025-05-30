﻿using Grasshopper2.Components;
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

[IoId("{49602452-908A-4FDA-992E-CB8C4E2D62C3}")]
[ComponentCategory("io", 0)]
public sealed class pcImportTXT : PancakeComponent<pcImportTXT>, IPancakeLocalizable<pcImportTXT>
{
    public static string StaticLocalizedName => Strings.ImportTXT;

    public static string StaticLocalizedDescription => Strings.ReadTextFileWithASpecificEncoding;

    public pcImportTXT() { }
    public pcImportTXT(IReader reader) : base(reader) { }
    protected override void RegisterInputs()
    {
        AddParam<TextParameter>("file");
        AddParam("encoding", "utf-8");
        AddParam("ok5", false);
    }

    protected override void RegisterOutputs()
    {
        AddParam<TextParameter>("text");
    }

    protected override void Process(IDataAccess access)
    {
        FailInUntrusted();

        if (Parameters.InputCount == 3)
        {
            access.GetItem(2, out bool ok);
            if (!ok) return;
        }

        access.GetItem(0, out string path);
        access.GetItem(1, out string encoding);

        if (!File.Exists(path))
        {
            access.AddError("Wrong file", Strings.FileDoesnTExist);
            return;
        }

        encoding ??= "default";
        encoding = encoding.ToLowerInvariant();

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
                access.SetItem(0, File.ReadAllText(path, encoder));
        }
        catch (Exception ex)
        {
            access.AddError("IO error", ex.Message);
        }
    }
    protected override IIcon? IconInternal => IconHost.CreateFromPathResource(pcExportTXT.IconResourceName);
}
