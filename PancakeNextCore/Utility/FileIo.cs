using PancakeNextCore.Dataset;
using System.IO;
using System.Text;

namespace PancakeNextCore.Utility;

/// <summary>
/// All IO operations should be fulfilled by methods in this class to centralize permission control.
/// </summary>
internal static class FileIo
{
    private const int MaxPathLength = 260;

    internal static bool IsValidPath(string path)
    {
        return !Config.IsInUntrustedMode && path.Length < MaxPathLength && File.Exists(path);
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
