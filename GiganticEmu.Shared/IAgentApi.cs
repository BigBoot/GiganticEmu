using System.Threading.Tasks;
using Refit;

namespace GiganticEmu.Shared
{
    public interface IAgentApi
    {
        [Get("/version")]
        Task<VersionGetResponse> GetVersion();

        [Post("/server")]
        Task<ServerPostResponse> StartServer([Body] ServerPostRequest request);
    }
}