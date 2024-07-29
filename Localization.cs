using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SettingsManager;

public static class Localization {
    const string FallbackLocale = "en";

    static Localization() {
        string locale = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        Stream fallback = ResourcesUtils.GetStream($"Localization/{FallbackLocale}.json")!;
        Stream current;

        try {
            current = ResourcesUtils.GetStream($"Localization/{locale}.json")!;
        } catch (IOException) {
            current = fallback;
        }

        Fallback = Json.Deserialize<Dictionary<string, string>>(fallback, true)!;
        Translations = Json.Deserialize<Dictionary<string, string>>(current, true)!;
    }

    static Dictionary<string, string> Fallback { get; }
    static Dictionary<string, string> Translations { get; }

    public static string GetString(string path) =>
        Translations.TryGetValue(path, out string value)
            ? value
            : Fallback[path];

    public static bool TryGetString(string path, out string value) =>
        Translations.TryGetValue(path, out value) ||
        Fallback.TryGetValue(path, out value);
}

public class LocalizationProvider {
    public readonly static DependencyProperty PathProperty =
        DependencyProperty.RegisterAttached(
            "Path",
            typeof(string),
            typeof(LocalizationProvider),
            new PropertyMetadata(string.Empty, OnPathChanged));

    public static string GetPath(DependencyObject obj) =>
        (string)obj.GetValue(PathProperty);

    public static void SetPath(DependencyObject obj, string value) =>
        obj.SetValue(PathProperty, value);

    static void OnPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
        string value = Localization.GetString((string)e.NewValue);

        switch (obj) {
            case TextBlock textBlock:
                textBlock.Text = value;
                break;

            case Run run:
                run.Text = value;
                break;

            case TabItem tabItem:
                tabItem.Header = value;
                break;
        }
    }
}
