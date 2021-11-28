namespace GiganticEmu.Shared;

public record FriendsDeleteRequest()
{
    public string UserName { get; init; } = default!;
}
