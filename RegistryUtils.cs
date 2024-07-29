using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Path = System.IO.Path;

namespace SettingsManager;

public static class RegistryUtils {
    static RegistryKey Software { get; } = Registry.CurrentUser.OpenSubKey("SOFTWARE")!;
    public static RegistryKey? Vint { get; } = Software.OpenSubKey("EELs Team")?.OpenSubKey("Vint", true);
    public static RegistryKey? RTX { get; } = Software.OpenSubKey("AlternativaPlatform")?.OpenSubKey("TankiX", true);

    public static Stream Export(this RegistryKey key) {
        string pathToFile = Path.GetTempFileName();

        Process process = new();
        process.EnableRaisingEvents = true;

        process.StartInfo = new ProcessStartInfo {
            FileName = "reg",
            Arguments = $"export \"{key.Name}\" \"{pathToFile}\" /y",
            UseShellExecute = false
        };

        process.Start();
        process.WaitForExit();

        MemoryStream stream = new();

        using (FileStream fileStream = File.OpenRead(pathToFile))
            fileStream.CopyTo(stream);

        stream.Position = 0;

        File.Delete(pathToFile);
        return stream;
    }

    public static object SetValue(this RegistryKey key, string name, object value, RegistryValueKind kind) {
        byte[] data = GetBytes(value);
        uint result = RegSetValueEx(key.Handle, name, 0, kind, data, (uint)data.Length);

        if (result != 0)
            throw new SystemException($"RegSetValueEx returned code {result}");

        return key.GetValue(name);
    }

    static byte[] GetBytes(object value) => value switch {
        bool boolean => BitConverter.GetBytes(boolean),
        char character => BitConverter.GetBytes(character),
        double @double => BitConverter.GetBytes(@double),
        float single => BitConverter.GetBytes(single),
        int int32 => BitConverter.GetBytes(int32),
        long int64 => BitConverter.GetBytes(int64),
        short int16 => BitConverter.GetBytes(int16),
        uint uint32 => BitConverter.GetBytes(uint32),
        ulong uint64 => BitConverter.GetBytes(uint64),
        ushort uint16 => BitConverter.GetBytes(uint16),
        byte[] buffer => buffer,
        _ => throw new ArgumentException($"Type {value.GetType()} is invalid")
    };

    [DllImport("advapi32.dll")]
    static extern uint RegSetValueEx(SafeRegistryHandle hKey, [Optional] string? lpValueName, [Optional] uint Reserved, RegistryValueKind dwType, byte[] lpData, uint cbData);
}
