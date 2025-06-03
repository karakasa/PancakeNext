using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;

namespace SvgIcon;
public static class DeflateHelper
{
#if DEBUG
    public static byte[] Compress(byte[] data)
    {
        using var dest = new MemoryStream();
        using var compressor = new DeflateStream(dest, CompressionLevel.Optimal);
        compressor.Write(data, 0, data.Length);
        compressor.Flush();

        return dest.ToArray();
    }
    public static byte[] Decompress(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var dest = new MemoryStream();
        using var compressor = new DeflateStream(ms, CompressionMode.Decompress);
        compressor.CopyTo(dest);

        return dest.ToArray();
    }
#endif
    public static byte[] Decompress(Stream data)
    {
        using var dest = new MemoryStream();
        using var compressor = new DeflateStream(data, CompressionMode.Decompress);
        compressor.CopyTo(dest);

        return dest.ToArray();
    }
}
