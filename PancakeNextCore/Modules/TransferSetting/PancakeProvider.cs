using System;
using System.IO;
using System.IO.Compression;
using Pancake.Dataset;
using Pancake.Utility;

namespace Pancake.Modules.TransferSetting
{
    public class PancakeProvider : Base
    {
        public readonly string[] Filenames = {"PancakeGH.xml"};

        public override Guid Guid => new Guid("ed9636ae-4a08-4e32-b5fc-48d634b07ffc");
        public override string FriendlyName => "Pancake";
        public override bool EnabledByDefault => true;
        public override bool RestartRequired => false;

        public override byte[] SaveToByteArary()
        {
            using (var mem = new MemoryStream())
            {
                using (var zip = new ZipArchive(mem, ZipArchiveMode.Create))
                {
                    foreach (var file in Filenames)
                    {
                        var path = Helper.TransferSetting.GetSettingName(file);
                        if (File.Exists(path))
                            zip.CreateEntryFromFile(path, file);
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
                foreach (var file in Filenames)
                {
                    var entry = zip.GetEntry(file);
                    if (entry == null)
                        continue;

                    File.WriteAllBytes(Helper.TransferSetting.GetSettingName(file), entry.Open().ReadToEnd());
                    switch (file)
                    {
                        case "PancakeGH.xml":
                            Config.Reload();
                            break;
                        default:
                            // Doesn't require special treatment
                            break;
                    }
                }
            }
            return true;
        }

        public override string Name => Strings.PancakeConfig;
        public override bool InternalModule => true;
    }
}
