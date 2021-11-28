namespace GiganticEmu.Shared;

public record ServerPostRequest
{
    public string Map { get; init; } = default!;
};
