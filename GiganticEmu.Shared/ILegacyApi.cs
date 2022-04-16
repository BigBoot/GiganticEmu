using System.Threading.Tasks;
using GiganticEmu.Shared.LegacyApi;
using Refit;

namespace GiganticEmu.Shared;

public interface ILegacyApi
{
    [Get("/version")]
    Task<VersionResponse> GetVersion();

    [Post("/start")]
    Task<StartResponse> StartServer([Body] StartRequest request);

    [Post("/kill")]
    Task<KillResponse> KillServer([Body] KillRequest request);

    [Post("/admin_pw")]
    Task<AdminPWResponse> AdminPW([Body] AdminPWRequest request);
}
