using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GiganticEmu.Launcher;

public class Settings
{
    public DateTime LastVersionCheck { get; set; } = DateTime.MinValue;

    public static void Load()
    {

        try
        {
            var location = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "config.json");
            var settings = JsonDocument.Parse(File.ReadAllText(location)).RootElement;
            foreach (var property in typeof(Settings).GetProperties(System.Reflection.BindingFlags.Static))
            {
                if (settings.TryGetProperty(property.Name, out var value))
                {
                    property.SetValue(null, JsonSerializer.Deserialize(value.GetRawText(), property.PropertyType));
                }
            }
        }
        catch (Exception) { }
    }

    public static void Save()
    {
        var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var settings = typeof(Settings)
            .GetProperties(System.Reflection.BindingFlags.Static)
            .ToDictionary(p => p.Name, p => p.GetValue(null));

        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Join(dir, "config.json"), JsonSerializer.Serialize(settings));
    }
}
