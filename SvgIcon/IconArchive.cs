using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvgIcon;
public sealed class IconArchive() : SmallArchive<PathIcon>("Pancake.SvgIcons")
{
    protected override PathIcon CreateFrom(BinaryReader reader, string name, int maxSize)
    {
        return PathIcon.CreateFrom(reader);
    }
#if DEBUG
    protected override void WriteTo(BinaryWriter writer, string name, PathIcon item)
    {
        item.WriteTo(writer);
    }
#endif
}
