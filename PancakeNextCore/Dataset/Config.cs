using Grasshopper2;
using System;
using System.IO;

namespace PancakeNextCore.Dataset;

internal static class Config
{
    const string DatabaseName = "PancakeGH";

    static readonly SettingsFile Database = SettingsFile.InDefaultFolder(DatabaseName);
    // internal static FeatureManager Features = null;

    internal static void LoadFromFile(string xml)
    {
    }
    internal static void Reload()
    {
    }

    public static void SaveToFile()
    {
        Database.TrySaveSettingsToFile();
    }

    internal static bool SafeMode = false;

    private static readonly bool _isRunningOnMac = Environment.OSVersion.Platform != PlatformID.Win32NT;
    internal static bool IsRunningOnMac => _isRunningOnMac;

    internal static string Read(string name, string def = "", string safeDef = "")
    {
        if (SafeMode)
            return string.IsNullOrEmpty(safeDef) ? def : safeDef;

        return Database.Get(name, def);
    }

    internal static bool Read(string name, bool def, bool safeDef)
    {
        try
        {
            return Convert.ToBoolean(Read(name, def.ToString(), safeDef.ToString()));
        }
        catch (Exception)
        {
            return SafeMode ? safeDef : def;
        }
    }

    internal static void Write(string key, string value)
    {
        Database.Set(key, value);
    }

    internal static void Write(string key, bool value)
    {
        Write(key, value.ToString());
    }

    internal static bool AlternativeMode => false;

    private static bool? _dev;

    private static bool? _preventSignature;

    private const string ConfigPreventGeneratingSignature = "PreventSig";
    public static bool PreventGeneratingSignature
    {
        get
        {
            return _preventSignature ??
                (_preventSignature = Read(ConfigPreventGeneratingSignature, false, false)).Value;
        }

        set
        {
            _preventSignature = value;
            Write(ConfigPreventGeneratingSignature, value);
        }
    }

    public static bool DevMode
    {
        get
        {
            if (!_dev.HasValue)
                _dev = Read("DevMode", false, false);
            return _dev.Value;
        }
        set
        {
            _dev = value;
            Write("DevMode", value.ToString());
        }
    }


    private static bool _untrustedFlagScanned = false;
    private static bool _inUntrusted = false;
    public static bool IsInUntrustedMode
    {
        get
        {
            if (!_untrustedFlagScanned)
                DetermineCloudMode();

            return _inUntrusted;
        }
    }

    private const string CloudModeFlagFile = "untrusted.pancake";
    private static void DetermineCloudMode()
    {
        var flagFile = Path.Combine(Path.GetDirectoryName(typeof(Config).Assembly.Location) ?? "", CloudModeFlagFile);
        if (File.Exists(flagFile))
        {
            _inUntrusted = true;
        }

        _untrustedFlagScanned = true;
    }

}
