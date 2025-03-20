using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PancakeNextCore.Dataset;

internal static class GuidDecompressor
{
    private static byte[] _cache = new byte[16];
    public static IEnumerable<Guid> GetGuids(byte[] storage)
    {
        using var memory = new MemoryStream(storage, false);

        var lastPos = storage.Length - 1;

        for (; ; )
        {
            if (memory.Position >= lastPos)
                break;
            if (memory.Read(_cache, 0, 16) < 16)
                break;

            yield return new Guid(_cache);
        }
    }
}
