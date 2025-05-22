using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SvgIcon;
public sealed class ByteArchive() : SmallArchive<byte[]>("Pancake.ByteArchive")
{
    public static ByteArchive CreateFrom(Stream stream)
    {
        var archive = new ByteArchive();
        archive.ReadFrom(stream);
        return archive;
    }
    protected override byte[] CreateFrom(BinaryReader reader, string name, int maxSize)
    {
        var length = reader.ReadInt32();
        var checksum = reader.ReadInt32();
        if (maxSize < sizeof(int) * 2 + length)
        {
            throw new InvalidDataException($"Malformed file {name}.");
        }

        var data = reader.ReadBytes(length);
        if (GetChecksum(data) != checksum)
        {
            // TODO
        }
        return data;
    }

    protected override void WriteTo(BinaryWriter writer, string name, byte[] item)
    {
        writer.Write(item.Length);
        writer.Write(GetChecksum(item));
        writer.Write(item);
    }

    private static int GetChecksum(byte[] data) => 0;
}
