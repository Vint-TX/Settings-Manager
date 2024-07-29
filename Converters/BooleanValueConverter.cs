using System;

namespace SettingsManager.Converters;

public class BooleanValueConverter(
    string trueLocalizationKey,
    string falseLocalizationKey
) : RegistryValueConverter {
    public override string GetFriendlyString(object? value) =>
        value == null
            ? "null"
            : Localization.GetString(Convert.ToBoolean(value)
                                         ? trueLocalizationKey
                                         : falseLocalizationKey);
}
