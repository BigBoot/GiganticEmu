namespace GiganticEmu.Shared;

public record UserPWPostRequest()
{
    public string UserName { get; init; } = default!;
    public string Token { get; init; } = default!;
    public string NewPassword { get; init; } = default!;
}
