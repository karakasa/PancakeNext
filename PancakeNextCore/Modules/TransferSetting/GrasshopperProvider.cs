using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using GH_IO.Types;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Pancake.Helper;
using Pancake.UI;
using Pancake.Utility;

namespace Pancake.Modules.TransferSetting
{
    public class GrasshopperProvider : Base
    {
        public readonly string[] Filenames =
        {
            "grasshopper_gui.xml", "grasshopper_kernel.xml", "markov_database.dat", "grasshopper_menushortcuts.xml",
            "grasshopper_gradients.xml", "grasshopper_userobject.xml", "grasshopper_ignorewarnings.xml", "grasshopper_materials.xml"
        };

        public override Guid Guid => new Guid("5f9aba7c-64c7-416a-ade9-21f29a16b95a");
        public override string FriendlyName => Strings.GrasshopperSettings;
        public override bool EnabledByDefault => true;
        public override bool RestartRequired => true;

        public override byte[] SaveToByteArary()
        {
            using (var mem = new MemoryStream())
            {
                using (var zip = new ZipArchive(mem, ZipArchiveMode.Create))
                {
                    zip.CreateEntry("gh_version").FromBytes(Encoding.ASCII.GetBytes(Versioning.Version.ToString()));
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

                foreach (var file in Filenames)
                {
                    entry = zip.GetEntry(file);
                    if (entry == null)
                        continue;

                    File.WriteAllBytes(Helper.TransferSetting.GetSettingName(file), entry.Open().ReadToEnd());
                    switch (file)
                    {
                        case "grasshopper_gui.xml":
                            GH_Skin.LoadSkin();
                            break;
                        case "markov_database.dat":
                            Instances.MarkovChain.ReadFromDisc();
                            break;
                        case "grasshopper_menushortcuts.xml":
                            ReflectionHelper.InvokeForeignMethod(Instances.DocumentEditor, "LoadCustomMenuShortcuts",
                                new object[] { });
                            break;
                        case "grasshopper_kernel.xml":
                            Instances.Settings.Reload();
                            break;
                        default:
                            // Doesn't require special treatment
                            break;
                    }
                }
            }
            return true;
        }

        public override string Name => Strings.GHSettings;
        public override bool InternalModule => true;
    }
}
