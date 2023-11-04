namespace GiganticEmu.ModdingToolkit;

public record PatchFunctionHEX : PatchHEX
{
    public required string Class { get; init; }
    public required string Function { get; init; }
}
