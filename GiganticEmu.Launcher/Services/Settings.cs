using System;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using GiganticEmu.Shared;

namespace GiganticEmu.Launcher;

public class Settings
{
    public enum Language
    {
        English,
        German,
        French,
    }

    public enum BackgroundImage
    {
        Leiran,
        Corruption,
        Eternals,
        Halloween,
    }

    public enum CompatiblityTool
    {
        Proton,
        Wine,
    }

    public BehaviorSubject<Language> GameLanguage { get; } = new(Language.English);

    public BehaviorSubject<String> OfflineName { get; } = new("");

    public BehaviorSubject<DateTime?> AutoUpdaterRemindLater { get; } = new(null);

    public BehaviorSubject<SemVer?> AutoUpdaterSkippedVersion { get; } = new(null);

    public BehaviorSubject<BackgroundImage> Background { get; } = new(BackgroundImage.Leiran);

    public BehaviorSubject<CompatiblityTool> LinuxCompatiblityTool { get; } = new(CompatiblityTool.Proton);

    public BehaviorSubject<bool> LinuxEnableGameMode { get; } = new(false);

    public BehaviorSubject<bool> LinuxEnableMangoHud { get; } = new(false);

    public async Task Load()
    {
        try
        {
            var location = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GiganticEmu", "GiganticEmu.Launcher.json");
            var settings = JsonDocument.Parse(await File.ReadAllTextAsync(location)).RootElement;
            foreach (var property in typeof(Settings).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                if (settings.TryGetProperty(property.Name, out var json))
                {
                    if (property.PropertyType.GetGenericTypeDefinition() != typeof(BehaviorSubject<>))
                    {
                        continue;
                    }

                    var type = property.PropertyType.GenericTypeArguments[0];
                    var subject = property.GetValue(this)!;
                    var setter = property.PropertyType.GetMethod("OnNext")!;
                    var value = JsonSerializer.Deserialize(json.GetRawText(), type);

                    setter.Invoke(subject, new[] { value });
                }
            }
        }
        catch (Exception) { }
    }

    public async Task Save()
    {
        var getter = typeof(BehaviorSubject<>).GetProperty("Value")!;
        var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var settings = typeof(Settings)
            .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .ToDictionary(p => p.Name, p => p.PropertyType.GetProperty("Value")!.GetValue(p.GetValue(this)));

        Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(Path.Join(dir, "GiganticEmu", "GiganticEmu.Launcher.json"), JsonSerializer.Serialize(settings));
    }
}