using System.Linq;

namespace SettingsManager;

public static class DJB2 {
    const int Magic = 5381;

    public static uint CalculateHash(string str) =>
        str.Aggregate<char, uint>(Magic, (hash, c) => (hash << 5) + hash ^ c);

    public static string GetNameWithHash(string name) =>
        $"{name}_h{CalculateHash(name)}";
}
