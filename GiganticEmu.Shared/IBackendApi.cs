using System.Threading.Tasks;
using Refit;

namespace GiganticEmu.Shared
{
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

        [Get("/friends")]
        Task<FriendsGetResponse> GetFriends();

        [Delete("/friends")]
        Task<FriendsDeleteResponse> RemoveFriend([Body] FriendsDeleteRequest request);

        [Post("/friends/invite")]
        Task<FriendsInvitePostResponse> SendFriendRequest([Body] FriendsInvitePostRequest request);
    }
}