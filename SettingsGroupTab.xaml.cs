using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SettingsManager.Settings;

namespace SettingsManager;

public partial class SettingsGroupTab {
    public SettingsGroupTab() => InitializeComponent();

    public SettingsGroup Group { get; set; }

    List<Setting> Settings { get; set; } = [];

    public void AddSetting(Setting setting) {
        setting.ValueChanged += ValidateValues;

        Settings.Add(setting);
        SettingsContainer.Children.Add(setting);

        ValidateValues();
    }

    void MoveToRTX(object sender, RoutedEventArgs e) {
        foreach (Setting setting in Settings.Where(setting => setting.MoveToRTXButton.IsEnabled))
            setting.MoveToRTX();

        ValidateValues();
    }

    void MoveToVint(object sender, RoutedEventArgs e) {
        foreach (Setting setting in Settings.Where(setting => setting.MoveToVintButton.IsEnabled))
            setting.MoveToVint();

        ValidateValues();
    }

    void ValidateValues() {
        MoveToRTXButton.IsEnabled = Settings.Any(s => s.MoveToRTXButton.IsEnabled);
        MoveToVintButton.IsEnabled = Settings.Any(s => s.MoveToVintButton.IsEnabled);
    }
}
