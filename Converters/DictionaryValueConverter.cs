using System.Collections.Generic;

namespace SettingsManager.Converters;

public class DictionaryValueConverter(
    Dictionary<string, string> variants,
    RegistryValueConverter? innerConverter = null,
    bool fallbackOnNotFound = false
) : RegistryValueConverter {
    public override string GetFriendlyString(object? value) {
        if (value == null)
            return Fallback(value);

        string key = innerConverter?.GetFriendlyString(value) ?? $"{value}";

        // ReSharper disable once InvertIf
        if (variants.TryGetValue(key, out string result)) {
            if (Localization.TryGetString(result, out string translation))
                return translation;

            if (fallbackOnNotFound)
                return result;
        }

        return fallbackOnNotFound
                   ? key
                   : throw new KeyNotFoundException($"Key {key} not found");
    }
}
