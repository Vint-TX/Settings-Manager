namespace SettingsManager.Converters;

public class SimpleValueConverter : RegistryValueConverter {
    public override string GetFriendlyString(object? value) =>
        value?.ToString() ?? "null";
}
