using SettingsManager.Converters;

namespace SettingsManager.Settings;

// ReSharper disable once ClassNeverInstantiated.Global
public class SettingDescription(
    string nameWithoutHash,
    string localizationKey,
    RegistryValueConverter valueConverter,
    SettingsGroup group,
    string[]? dependencies = null,
    string? tipLocalizationKey = null
) {
    public string NameWithoutHash { get; } = nameWithoutHash;
    public string NameWithHash => DJB2.GetNameWithHash(NameWithoutHash);
    public string FriendlyName => Localization.GetString(localizationKey);
    public string? Tip => tipLocalizationKey != null
                              ? Localization.GetString(tipLocalizationKey)
                              : null;

    public RegistryValueConverter ValueConverter { get; } = valueConverter;
    public SettingsGroup Group { get; } = group;

    public string[] Dependencies { get; } = dependencies ?? [];
}
