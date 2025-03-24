namespace PancakeNextCore.Interfaces;

public interface IPancakeLocalizable
{
    string LocalizedName { get; }
    string LocalizedDescription { get; }

    string? LastSaveLocalization { get; }
    void RefreshLocalizationAppearance();
}
