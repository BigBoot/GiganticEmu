using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Json.Schema;
using Json.Schema.Serialization;

namespace GiganticEmu.ModdingToolkit;


[JsonSchema(typeof(Mod), nameof(Schema))]
public record Mod
{
    public static JsonSchema Schema = JsonSchema.FromText(
        new StreamReader(Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("GiganticEmu.ModdingToolkit.Resources.GiganticEmu.Mod.Schema.json")!
        ).ReadToEnd());

    public required string Description { get; init; }
    public required string Author { get; init; }
    public required List<int> Builds { get; init; }
    public required List<Patch> Patches { get; init; }
}
