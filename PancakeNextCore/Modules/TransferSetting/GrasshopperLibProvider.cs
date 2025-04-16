using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using GH_IO.Types;
using Grasshopper;
using Pancake.UI;
using Pancake.Utility;

namespace Pancake.Modules.TransferSetting
{
    public class GrasshopperLibProvider : Base
    {
        public readonly string[] Directories =
        {
            "Libraries", "UserObjects", "6/Libraries"
        };

        public override Guid Guid => new Guid("ad634212-be4c-4507-a1b4-a62dceddb2a4");
        public override string FriendlyName => Strings.GrasshopperAddOns;
        public override bool EnabledByDefault => false;
        public override bool RestartRequired => true;

        public override byte[] SaveToByteArary()
        {
            using (var mem = new MemoryStream())
            {
                using (var zip = new ZipArchive(mem, ZipArchiveMode.Create))
                {
                    zip.CreateEntry("gh_version").FromBytes(Encoding.ASCII.GetBytes(Versioning.Version.ToString()));
                    foreach (var folder in Directories)
                    {
                        var libPath = Path.Combine(Folders.SettingsFolder, folder) + Path.DirectorySeparatorChar;
                        FileSystem.WalkThroughDirectory(libPath,
                            path => zip.CreateEntryFromFile(path,
                                FileSystem.GetRelativePath(Folders.SettingsFolder, path)));
                    }
                }

                return mem.ToArray();
            }
        }

        public override bool LoadFromByteArray(byte[] setting)
        {
            using (var mem = new MemoryStream(setting))
            using (var zip = new ZipArchive(mem, ZipArchiveMode.Read))
            {
                var entry = zip.GetEntry("gh_version");
                if (entry == null)
                    return false;
                var ghVersion = Encoding.ASCII.GetString(entry.Open().ReadToEnd()).Split('.');
                if (ghVersion.Length != 3)
                    return false;
                var versionInFile = new GH_Version(Convert.ToInt32(ghVersion[0]), Convert.ToInt32(ghVersion[1]),
                    Convert.ToInt32(ghVersion[2]));

                if (versionInFile > Versioning.Version)
                    if (!Helper.TransferSetting.QuietMode && !UiHelper.ContinueWarning(
                            Strings.GrasshopperProvider_TransferWarning))
                        return false;

                var basePath = Folders.SettingsFolder;
                var failList = new List<string>();
                foreach (var file in zip.Entries)
                {
                    if (file.FullName == file.Name)
                        continue;

                    try
                    {
                        var path = Path.Combine(basePath, file.FullName);
                        var data = file.Open().ReadToEnd();
                        if (File.Exists(path))
                        {
                            if (new FileInfo(path).Length == data.LongLength)
                            {
                                var data2 = File.ReadAllBytes(path);
                                if (UnsafeHelper.CompareByteArray(data, data2))
                                {
                                    data2 = null;
                                    continue;
                                }
                                data2 = null;
                            }

                            File.Delete(path);
                        }

                        var dir = Path.GetDirectoryName(path);
                        if(dir != null)
                            Directory.CreateDirectory(dir);
                        File.WriteAllBytes(path, data);
                        data = null;
                    }
                    catch (Exception e)
                    {
                        failList.Add(file.FullName);
                    }
                }

                if (!Helper.TransferSetting.QuietMode && failList.Count != 0)
                    UiHelper.Information(Strings.GrasshopperLibProvider_LoadFromByteArray_CannotOverride +
                                         string.Join("\r\n", failList));
            }
            return true;
        }

        public override string Name => Strings.GHLibraries;
        public override bool InternalModule => true;
    }
}
