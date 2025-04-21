namespace PancakeNextCore.Interfaces;

public interface IPancakeLocalizable
{
    string LocalizedName { get; }
    string LocalizedDescription { get; }

    string? LastSaveLocalization { get; }
    void RefreshLocalizationAppearance();
}

public interface IPancakeLocalizable<T> : IPancakeLocalizable
    where T : IPancakeLocalizable<T>
{
#if NET
    public static abstract string StaticLocalizedName { get; }
    public static abstract string StaticLocalizedDescription { get; }
#endif
}
