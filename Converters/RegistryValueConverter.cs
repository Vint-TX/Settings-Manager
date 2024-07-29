namespace SettingsManager.Converters;

public abstract class RegistryValueConverter {
    static SimpleValueConverter FallbackConverter { get; } = new();

    public abstract string GetFriendlyString(object? value);

    protected static string Fallback(object? value) =>
        FallbackConverter.GetFriendlyString(value);
}
