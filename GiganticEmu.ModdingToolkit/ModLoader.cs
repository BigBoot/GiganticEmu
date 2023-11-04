using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using Json.Schema;
using Json.Schema.Serialization;

namespace GiganticEmu.ModdingToolkit;

public class ModLoader
{
    public record LoadFailedEventArgs
    {
        public required string Name { get; init; }
        public required string Message { get; init; }
    }

    public delegate void LoadFailedEventHandler(object sender, LoadFailedEventArgs a);

    public event LoadFailedEventHandler? LoadFailed;

    static ModLoader()
    {
        SchemaRegistry.Global.Fetch = (uri) =>
        {
            Console.WriteLine(uri);
            return null;
        };
    }

    private readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = {
            new JsonStringEnumConverter(),
            new ValidatingJsonConverter()
            {
                OutputFormat = OutputFormat.List
            },
            Patch.Converter,
        }
    };

    public async Task<IEnumerable<Mod>> LoadMods()
    {
        var mods = await Task.WhenAll(new[] {
            LoadModsFromResources(),
            LoadModsFromDirectory(Path.Join(GameUtils.GetBaseDir(), "Mods"))
        });
        return mods.SelectMany(x => x);
    }

    public async Task<Mod?> LoadMod(FileInfo file)
    {
        using var stream = file.OpenRead();
        return await LoadMod(file.FullName, stream);
    }

    private async Task<Mod?> LoadMod(string name, Stream stream)
    {
        try
        {
            return await JsonSerializer.DeserializeAsync<Mod>(stream, jsonOptions);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);

            var message = e switch
            {
                JsonException je when je.Data.Contains("validation") => je.Data["validation"] switch
                {
                    EvaluationResults result => string.Join("\n", result.Details
                        .SelectMany(details => details.Errors
                            ?.Select(error => $"{details.InstanceLocation.Combine(error.Key)}: {error.Value}")
                            ?? Enumerable.Empty<string>()
                        )
                    ),
                    _ => null,
                },
                _ => null,
            } ?? e.Message;

            LoadFailed?.Invoke(this, new LoadFailedEventArgs
            {
                Name = name,
                Message = message,
            });
            return null;
        }
    }

    private async Task<IEnumerable<Mod>> LoadModsFromResources()
    {
        var assembly = Assembly.GetExecutingAssembly();

        return await Task.WhenAll(
            assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith("GiganticEmu.ModdingToolkit.Resources.Mods."))
                .Select(x => new { Name = x, Stream = assembly.GetManifestResourceStream(x) })
                .Select(x => x switch
                {
                    { Name: string name, Stream: Stream stream } => LoadMod(name, stream),
                })
                .OfType<Task<Mod>>()
            );
    }

    private async Task<IEnumerable<Mod>> LoadModsFromDirectory(string dir)
    {
        if (!Directory.Exists(dir))
        {
            return Enumerable.Empty<Mod>();
        }

        return await Task.WhenAll(
            new DirectoryInfo(dir)
                .GetFiles()
                .Select(LoadMod)
                .OfType<Task<Mod>>()
        );
    }
}