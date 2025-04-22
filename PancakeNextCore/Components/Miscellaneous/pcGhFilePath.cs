using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Interfaces;
using System;
using System.IO;
using Path = System.IO.Path;

namespace PancakeNextCore.Components.Miscellaneous;

[ComponentCategory("io", 2)]
[IoId("1716e4e4-d58f-4713-91ba-919e7ed8b728")]
public sealed class pcGhFilePath : PancakeComponent<pcGhFilePath>, IPancakeLocalizable<pcGhFilePath>
{
    public pcGhFilePath() { }
    public pcGhFilePath(IReader reader) : base(reader) { }

    public static string StaticLocalizedName => Strings.DefinitionPath;
    public static string StaticLocalizedDescription => Strings.GetThePathOfCurrentGrasshopperDefinitionFileSoThatYouCanReferenceFilesWhereverTheyAre;
    protected override void RegisterInputs()
    {
    }

    protected override void RegisterOutputs()
    {
        AddParam<TextParameter>("directory");
        AddParam<TextParameter>("name");
        AddParam<TextParameter>("directoryseparator");
    }

    protected override void Process(IDataAccess access)
    {
        FailInUntrusted();

        access.SetItem(2, Path.DirectorySeparatorChar.ToString());

        var doc = Document ?? (Editor.Instance?.Canvas?.Document);
        if (doc is null)
            return;

        for (; ; )
        {
            if (doc.Parent is not Document doc2 || doc2 == doc) break;
            doc = doc2;
        }

        var file = doc?.File;
        if (file is null || !file.HasPath)
        {
            access.AddError("Wrong file", Strings.FileHasnTBeenSavedOrTheComponentIsInsideACluster);
            return;
        }

        var path = file.Path;

        access.SetItem(0, Path.GetDirectoryName(path));
        access.SetItem(1, Path.GetFileName(path));
    }
}