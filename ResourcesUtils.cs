using System;
using System.IO;
using System.Windows;

namespace SettingsManager;

public static class ResourcesUtils {
    public static Stream? GetStream(string path, UriKind uriKind = UriKind.Relative) =>
        Application.GetResourceStream(new Uri(path, uriKind))?.Stream;
}
