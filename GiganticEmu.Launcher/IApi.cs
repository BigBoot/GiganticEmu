using System.Threading.Tasks;
using GiganticEmu.Shared;
using Refit;

namespace GiganticEmu.Launcher
{
    public interface IApi
    {

        [Post("/session")]
        Task<SessionPostResponse> Login([Body] SessionPostRequest request);

        [Delete("/session")]
        Task<SessionDeleteResponse> Logout([Body] SessionDeleteRequest request);

        [Get("/session")]
        Task<SessionGetResponse> GetSession();

        [Post("/user")]
        Task<UserPostResponse> Register([Body] UserPostRequest request);
    }
}