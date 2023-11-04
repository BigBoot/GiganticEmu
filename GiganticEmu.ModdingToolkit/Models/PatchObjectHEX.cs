namespace GiganticEmu.ModdingToolkit;

public record PatchObjectHEX : PatchHEX
{
    public required string Object { get; init; }
}
