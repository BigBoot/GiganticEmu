namespace GiganticEmu.Shared;

public record FriendsInvitePostRequest()
{
    public string UserName { get; init; } = default!;
}
