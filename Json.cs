using System.IO;
using Newtonsoft.Json;

namespace SettingsManager;

public static class Json {
    public static T? Deserialize<T>(Stream stream, bool disposeStream = false) {
        using StreamReader reader = new(stream);
        string data = reader.ReadToEnd();

        if (disposeStream)
            stream.Dispose();

        return JsonConvert.DeserializeObject<T>(data, new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Auto
        });
    }
}
