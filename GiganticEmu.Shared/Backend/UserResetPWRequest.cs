namespace GiganticEmu.Shared;

public record UserPWDeleteRequest()
{
    public string UserName { get; init; } = default!;
}
