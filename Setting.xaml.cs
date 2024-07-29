using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using SettingsManager.Converters;

namespace SettingsManager;

public partial class Setting {
    object? _vintValue;
    object? _rtxValue;

    public Setting(
        string name,
        string registryKeyWithoutHash,
        string registryKey,
        object? vintValue,
        object? rtxValue,
        RegistryValueConverter valueConverter,
        string? tip = null) {
        InitializeComponent();

        RegistryKeyWithoutHash = registryKeyWithoutHash;
        RegistryKey = registryKey;
        ValueConverter = valueConverter;
        SettingName.Text = name;
        VintValue = vintValue;
        RTXValue = rtxValue;

        if (string.IsNullOrWhiteSpace(tip))
            return;

        TooltipIcon.Visibility = Visibility.Visible;
        TipText.Text = tip;
    }

    object? VintValue {
        get => _vintValue;
        set {
            _vintValue = value;
            VintValueText.Text = ValueConverter.GetFriendlyString(value);

            ValidateValues();

            ValueChanged?.Invoke();
        }
    }

    object? RTXValue {
        get => _rtxValue;
        set {
            _rtxValue = value;
            RTXValueText.Text = ValueConverter.GetFriendlyString(value);

            ValidateValues();

            ValueChanged?.Invoke();
        }
    }

    public event Action? ValueChanged;

    public string RegistryKeyWithoutHash { get; }
    public string RegistryKey { get; }
    Setting[] Dependencies { get; set; } = [];
    RegistryValueConverter ValueConverter { get; }

    public void InitDependencies(Setting[] dependencies) =>
        Dependencies = dependencies;

    public void MoveToRTX(params Setting[] excludedDeps) {
        object newValue = RegistryUtils.SetValue(RegistryUtils.RTX!, RegistryKey, VintValue!, RegistryUtils.Vint!.GetValueKind(RegistryKey));
        RTXValue = newValue;

        foreach (Setting dependency in Dependencies.Where(dependency => !excludedDeps.Contains(dependency)))
            dependency.MoveToRTX([..excludedDeps, this]);
    }

    public void MoveToVint(params Setting[] excludedDeps) {
        object newValue = RegistryUtils.SetValue(RegistryUtils.Vint!, RegistryKey, RTXValue!, RegistryUtils.RTX!.GetValueKind(RegistryKey));
        VintValue = newValue;

        foreach (Setting dependency in Dependencies.Where(dependency => !excludedDeps.Contains(dependency)))
            dependency.MoveToVint([..excludedDeps, this]);
    }

    void ValidateValues() {
        MoveToRTXButton.IsEnabled = MoveToVintButton.IsEnabled = !ValuesEquals(RTXValue, VintValue);

        if (RTXValue == null || RegistryUtils.Vint == null)
            MoveToVintButton.IsEnabled = false;

        if (VintValue == null || RegistryUtils.RTX == null)
            MoveToRTXButton.IsEnabled = false;
    }

    void MoveToRTXPressed(object sender, RoutedEventArgs e) => MoveToRTX();

    void MoveToVintPressed(object sender, RoutedEventArgs e) => MoveToVint();

    static bool ValuesEquals(object? a, object? b) {
        if (a is byte[] aBuffer && b is byte[] bBuffer)
            return aBuffer.Length == bBuffer.Length && memcmp(aBuffer, bBuffer, aBuffer.Length) == 0;

        return Equals(a, b);
    }

    [DllImport("msvcrt.dll", CallingConvention=CallingConvention.Cdecl)]
    static extern int memcmp(byte[] b1, byte[] b2, long count);
}
