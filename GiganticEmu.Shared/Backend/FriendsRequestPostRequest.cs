namespace GiganticEmu.Shared;

public record FriendsRequestPostRequest()
{
    public string UserName { get; init; } = default!;
}
