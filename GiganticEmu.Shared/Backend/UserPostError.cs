namespace GiganticEmu.Shared;

public record UserPostError()
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;
}
