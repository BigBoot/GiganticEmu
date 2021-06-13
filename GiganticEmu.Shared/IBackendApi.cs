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
    }
}