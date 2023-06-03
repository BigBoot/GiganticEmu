using System.Threading.Tasks;
using Refit;

namespace GiganticEmu.Shared;

public interface IBackendApi
{

    [Post("/session")]
    Task<SessionPostResponse> Login([Body] SessionPostRequest request);

    [Delete("/session")]
    Task<SessionDeleteResponse> Logout([Body] SessionDeleteRequest request);

    [Get("/session")]
    Task<SessionGetResponse> GetSession();

    [Post("/user")]
    Task<UserPostResponse> Register([Body] UserPostRequest request);

    [Delete("/user/pw")]
    Task<UserPWDeleteResponse> ResetPassword([Body] UserPWDeleteRequest request);

    [Post("/user/pw")]
    Task<UserPWPostResponse> ChangePassword([Body] UserPWPostRequest request);

    [Get("/friends")]
    Task<FriendsGetResponse> GetFriends();

    [Delete("/friends")]
    Task<FriendsDeleteResponse> RemoveFriend([Body] FriendsDeleteRequest request);

    [Post("/friends/request")]
    Task<FriendsRequestPostResponse> SendFriendRequest([Body] FriendsRequestPostRequest request);

    [Post("/friends/invite")]
    Task<FriendsInvitePostResponse> SendInvite([Body] FriendsInvitePostRequest request);

    [Post("/discord/token")]
    Task<DiscordPostTokenResponse> LinkDiscord([Body] DiscordPostTokenRequest request);
}
