using System.Text;

namespace SettingsManager.Converters;

public class StringValueConverter : RegistryValueConverter {
    public override string GetFriendlyString(object? value) =>
        value is byte[] buffer
            ? Encoding.UTF8.GetString(buffer)
            : Fallback(value);
}
