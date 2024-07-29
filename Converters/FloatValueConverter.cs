using System;
using System.Globalization;

namespace SettingsManager.Converters;

public class FloatValueConverter : RegistryValueConverter {
    public override string GetFriendlyString(object? value) =>
        value is long number
            ? Math.Round(BitConverter.Int64BitsToDouble(number), 3).ToString(CultureInfo.CurrentCulture)
            : Fallback(value);
}
