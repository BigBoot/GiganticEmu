namespace GiganticEmu.ModdingToolkit;

public record PatchHEX : Patch
{

    public enum PatchBehavior
    {
        First,
        Last,
        All,
        Single,
    }

    public required string File { get; init; }
    public required string Before { get; init; }
    public required string After { get; init; }
    public PatchBehavior Behavior { get; init; } = PatchBehavior.Single;
}
