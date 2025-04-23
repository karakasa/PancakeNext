using PancakeNextCore.Dataset;
using System.IO;
using System.Text;

namespace PancakeNextCore.Utility;

/// <summary>
/// All IO operations should be fulfilled by methods in this class to centralize permission control.
/// </summary>
internal static class FileIo
{
    private const int MaxPathLength = 1024;

    internal static bool IsValidPath(string path)
    {
        return !Config.IsInUntrustedMode && path.Length < MaxPathLength &&
            IsAbsPathPrefixAccepted(path) && File.Exists(path);
    }

    private static bool IsAbsPathPrefixAccepted(string path)
    {
        if (path.Length < 4) return false;

        var c0 = path[0];
        var c1 = path[1];
        var c2 = path[2];
        if (c1 is ':' && (c2 is '/' or '\\')) return true;
        if (c0 == c1 && c0 is '\\') return true;
        if (c0 is '/' or '\\' && !(c1 is '/' or '\\')) return true;

        return false;
    }

    internal static string ReadContentIfIsFile(string content)
    {
        if (IsValidPath(content))
            return File.ReadAllText(content);

        return content;
    }

    internal static string ReadAllText(string path)
    {
        if (Config.IsInUntrustedMode)
            return string.Empty;

        return File.ReadAllText(path);
    }

    internal static string ReadAllText(string path, Encoding encoding)
    {
        if (Config.IsInUntrustedMode)
            return string.Empty;

        return File.ReadAllText(path, encoding);
    }

    public static string? GetInvariantName(this string path)
    {
        if (path is null) return null;
        return Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
    }
}
