using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GiganticEmu.Shared;

public class ApiTokenHandler : DelegatingHandler
{
    public string? AuthToken { get; set; } = null;

    public ApiTokenHandler(HttpMessageHandler? innerHandler = null)
        : base(innerHandler ?? new HttpClientHandler())
    {

    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (AuthToken is string token)
        {
            request.Headers.Add("X-API-Key", token);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
