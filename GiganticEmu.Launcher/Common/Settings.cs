using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GiganticEmu.Launcher;

public class Settings
{
    public static String GameLanguage { get; set; } = "";

    public static void Load()
    {

        try
        {
            var location = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GiganticEmu", "GiganticEmu.Launcher.json");
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
            .GetProperties(System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public)
            .ToDictionary(p => p.Name, p => p.GetValue(null));

        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Join(dir, "GiganticEmu", "GiganticEmu.Launcher.json"), JsonSerializer.Serialize(settings));
    }
}