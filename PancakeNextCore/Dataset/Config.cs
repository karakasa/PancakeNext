using Grasshopper2;
using Rhino;
using System;
using System.Diagnostics;
using System.IO;

namespace PancakeNextCore.Dataset;

public static class Config
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

    public static bool IsRunningOnMac { get; } = Environment.OSVersion.Platform != PlatformID.Win32NT;
    public static bool IsRunningOnModernNET { get; } = Environment.Version.Major >= 5;
    public const bool IsCompiledForModernNET =
#if NET
        true
#else
        false
#endif
        ;

    public static bool IsModernNETMismatched => IsRunningOnModernNET ^ IsCompiledForModernNET;

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

    internal static void Write(string key, string? value)
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

    internal static bool DevMode
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

    private static RunningEnvironment? _env;
    private static RunningEnvironment DetermineRunningEnvironment()
    {
        try
        {
            if (IsRunningOnMac)
                return RunningEnvironment.Rhino;

            var prcName = Process.GetCurrentProcess()?.ProcessName?.ToLowerInvariant();
            switch (prcName)
            {
                case "rhino":
                    return RunningEnvironment.Rhino;
                case "revit":
                case "acad":
                    return RunningEnvironment.RhinoInside;
                case "rhino.compute":
                    return RunningEnvironment.RhinoCompute;
            }
        }
        catch
        {
        }

        return DetermineRunningEnvironmentFallback();
    }
    private static RunningEnvironment DetermineRunningEnvironmentFallback()
    {
        if (RhinoApp.IsRunningHeadless)
            return RunningEnvironment.UnknownHeadless;
        return RunningEnvironment.Unknown;
    }
    public static RunningEnvironment CurrentEnvironment => _env ??= DetermineRunningEnvironment();
    public static bool IsInRhino => CurrentEnvironment is RunningEnvironment.Rhino;
    public static bool IsInteractive => CurrentEnvironment is RunningEnvironment.Rhino or RunningEnvironment.RhinoInside;
}

public enum RunningEnvironment
{
    Rhino,
    RhinoInside,
    RhinoCompute,
    UnknownHeadless,
    Unknown
}
