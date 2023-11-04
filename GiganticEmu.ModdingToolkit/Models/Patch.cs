using System.Text.Json.Serialization;

namespace GiganticEmu.ModdingToolkit;

public interface Patch
{
    public static JsonPolymorphicConverter<Patch> Converter = new(
            discriminatorPropertyName: "Type",
            new()
            {
                [nameof(PatchFunctionHEX)] = typeof(PatchFunctionHEX),
                [nameof(PatchObjectHEX)] = typeof(PatchObjectHEX),
            }
        );
};
