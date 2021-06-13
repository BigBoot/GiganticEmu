using System.Threading.Tasks;
using GiganticEmu.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GiganticEmu.Agent
{
    [ApiController]
    [Route("/")]
    [RequireApiKey]
    public class ApiController : ControllerBase
    {
        private const int API_VERSION = 1;

        private readonly ILogger<ServerManager> _logger;
        private readonly AgentConfiguration _configuration;
        private readonly ServerManager _serverManager;

        public ApiController(ILogger<ServerManager> logger, IOptions<AgentConfiguration> configuration, ServerManager serverManager)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _serverManager = serverManager;
        }

        [HttpGet("version")]
        [Produces("application/json")]
        public async Task<IActionResult> GetVersion()
        {
            return Ok(new VersionGetResponse(RequestResult.Success)
            {
                ApiVersion = API_VERSION,
                AgentVersionMajor = 0,
                AgentVersionMinor = 0,
                AgentVersionPatch = 0
            });
        }

        [HttpPost("server")]
        [Produces("application/json")]
        public async Task<IActionResult> PostServer(ServerPostRequest start)
        {
            var port = await _serverManager.StartInstance(start.Map);
            return Ok(new ServerPostResponse(port == 0 ? RequestResult.NoInstanceAvailable : RequestResult.Success)
            {
                Port = port
            });
        }
    }
}