using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Win32;
using SettingsManager.Converters;
using SettingsManager.Settings;

namespace SettingsManager;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow {
    public MainWindow() {
        if (RegistryUtils.Vint == null && RegistryUtils.RTX == null) {
            MessageBox.Show(Localization.GetString("NoGameInstallationFound"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
            return;
        }

        InitializeComponent();

        if (Environment.GetCommandLineArgs().Contains("show-all")) {
            ItemCollection items = Tabs.Items;
            SettingsGroupTab tab = new() { Group = SettingsGroup.Other };

            LocalizationProvider.SetPath(tab, "OtherGroup");

            items.Insert(items.Count - 1, tab);
        }

        MessageBoxResult result = MessageBox.Show(Localization.GetString("BackupWarning"),
            "Warning",
            MessageBoxButton.YesNo,
            MessageBoxImage.Exclamation,
            MessageBoxResult.Yes);

        if (result == MessageBoxResult.Yes)
            ShowSaveBackupDialog();

        CreateSettings();
    }

    void CreateSettings() {
        SettingDescription[] descriptions = Json.Deserialize<SettingDescription[]>(ResourcesUtils.GetStream("Settings/descriptions.json")!, true)!;
        SettingsGroupTab[] settingsTabs = Tabs.Items.OfType<SettingsGroupTab>().ToArray();

        List<Setting> collected = new(descriptions.Length);

        foreach (SettingDescription description in descriptions) {
            object? vintValue = RegistryUtils.Vint?.GetValue(description.NameWithHash);
            object? rtxValue = RegistryUtils.RTX?.GetValue(description.NameWithHash);

            Setting setting = new(description.FriendlyName,
                description.NameWithoutHash,
                description.NameWithHash,
                vintValue,
                rtxValue,
                description.ValueConverter,
                description.Tip);

            settingsTabs.First(tab => tab.Group == description.Group).AddSetting(setting);
            collected.Add(setting);
        }

        SettingsGroupTab? otherTab = settingsTabs.FirstOrDefault(tab => tab.Group == SettingsGroup.Other);

        string[] vintNames = RegistryUtils.Vint?.GetValueNames() ?? [];
        string[] rtxNames = RegistryUtils.RTX?.GetValueNames() ?? [];

        string[] valueNames = vintNames
            .Concat(rtxNames)
            .Except(collected.Select(setting => setting.RegistryKey))
            .OrderBy(x => x)
            .ToArray();

        foreach (string name in valueNames) {
            string nameWithoutHash = name.Substring(0, name.IndexOf("_h", StringComparison.Ordinal));

            object? vintValue = RegistryUtils.Vint?.GetValue(name);
            object? rtxValue = RegistryUtils.RTX?.GetValue(name);

            Setting setting = new(nameWithoutHash, nameWithoutHash, name, vintValue, rtxValue, new SimpleValueConverter());
            otherTab?.AddSetting(setting);

            collected.Add(setting);
        }

        foreach (SettingDescription description in descriptions) {
            Setting parent = collected.First(setting => setting.RegistryKeyWithoutHash == description.NameWithoutHash);

            Setting[] dependencies = description.Dependencies
                .Select(name => collected.First(setting => setting.RegistryKeyWithoutHash == name))
                .ToArray();

            parent.InitDependencies(dependencies);
        }
    }

    static void ShowSaveBackupDialog() {
        SaveFileDialog backupDialog = new() {
            FileName = "Backup.reg",
            Filter = $"{Localization.GetString("RegistryFilesFilter")}|*.reg",
            AddExtension = true,
            DefaultExt = "reg",
            OverwritePrompt = true
        };

        backupDialog.FileOk += BackupDialogSubmitted;
        backupDialog.ShowDialog();
    }

    static void BackupDialogSubmitted(object sender, CancelEventArgs e) {
        SaveFileDialog dialog = (SaveFileDialog)sender;
        Stream? vintBackup = PrepareBackup(RegistryUtils.Vint?.Export());
        Stream? rtxBackup = PrepareBackup(RegistryUtils.RTX?.Export());

        Stream source;

        if (vintBackup != null && rtxBackup != null) {
            source = MergeBackups(vintBackup, rtxBackup);
            vintBackup.Dispose();
            rtxBackup.Dispose();
        } else if (vintBackup != null)
            source = vintBackup;
        else if (rtxBackup != null)
            source = rtxBackup;
        else
            throw new InvalidOperationException();

        using Stream output = dialog.OpenFile();

        source.CopyTo(output);
        source.Dispose();
    }

    static Stream? PrepareBackup(Stream? stream) {
        if (stream == null)
            return null;

        List<string> content = ReadAllLines(stream).ToList();

        string partition = content[2];

        if (!partition.StartsWith("[") || !partition.EndsWith("]"))
            throw new ArgumentException();

        partition = partition.Insert(1, "-");
        content.Insert(2, partition);

        return new MemoryStream(Encoding.Default.GetBytes(string.Join(Environment.NewLine, content)));
    }

    static Stream MergeBackups(Stream first, Stream second) {
        string[] firstContent = ReadAllLines(first);
        string[] secondContent = ReadAllLines(second);
        string result = string.Join(Environment.NewLine, firstContent.Concat(secondContent.Skip(1)));

        return new MemoryStream(Encoding.Default.GetBytes(result));
    }

    static string[] ReadAllLines(Stream stream) {
        List<string> stringList = [];

        using (StreamReader streamReader = new(stream)) {
            while (streamReader.ReadLine() is { } str)
                stringList.Add(str);
        }

        return stringList.ToArray();
    }

    protected override void OnSourceInitialized(EventArgs e) {
        base.OnSourceInitialized(e);

        IconHelper.RemoveIcon(this);
    }

    void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e) =>
        Process.Start(e.Uri.ToString());
}
