using System;
using System.Text;
using Microsoft.Win32;

namespace SettingsManager;

public class PlayerPrefs(
    RegistryKey key
) {
    public bool GetBool(string nameWithoutHash) =>
        Convert.ToBoolean(GetValue(nameWithoutHash));

    public int GetInt(string nameWithoutHash) =>
        Convert.ToInt32(GetValue(nameWithoutHash));

    public float GetFloat(string nameWithoutHash) =>
        (float)BitConverter.Int64BitsToDouble(GetLong(nameWithoutHash));

    public string GetString(string nameWithoutHash) =>
        Encoding.UTF8.GetString(GetBytes(nameWithoutHash));

    long GetLong(string nameWithoutHash) =>
        (long)GetValue(nameWithoutHash);

    byte[] GetBytes(string nameWithoutHash) =>
        (byte[])GetValue(nameWithoutHash);

    object GetValue(string nameWithoutHash) =>
        key.GetValue(DJB2.GetNameWithHash(nameWithoutHash));
}
