using System;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using GiganticEmu.Shared.LegacyApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GiganticEmu.Agent;

[ApiController]
[Route("/api")]
[RequireApiKey]
public class LegacyApiController : ControllerBase
{
    private const int API_VERSION = 2;

    private readonly ILogger<ServerManager> _logger;
    private readonly AgentConfiguration _configuration;
    private readonly ServerManager _serverManager;

    public LegacyApiController(ILogger<ServerManager> logger, IOptions<AgentConfiguration> configuration, ServerManager serverManager)
    {
        _logger = logger;
        _configuration = configuration.Value;
        _serverManager = serverManager;
    }

    [HttpGet("version")]
    [HttpPost("version")]
    [Produces("application/json")]
    public async Task<IActionResult> GetVersion()
    {
        return Ok(new VersionResponse
        {
            ApiVersion = API_VERSION,
            AppVersionMajor = 0,
            AppVersionMinor = 0,
            AppVersionPatch = 0,
            AppVersion = $"{0}.{0}.{0}"
        });
    }

    [HttpGet("logs")]
    [Produces("application/json")]
    public async Task<IActionResult> GetLogs([FromQuery(Name = "id")] int _id, [FromQuery(Name = "from_line")] long? _from_line, [FromQuery(Name = "to_line")] long? _to_line)
    {
        return Ok(new string[0]);
    }

    [HttpGet("players")]
    [Produces("application/json")]
    public async Task<IActionResult> GetPlayers([FromQuery(Name = "id")] int _id)
    {
        return Ok(new object[0]);
    }

    [HttpPost("start")]
    [Produces("application/json")]
    public async Task<IActionResult> PostStart(StartRequest req)
    {
        try
        {
            var port = await _serverManager.StartInstance(req.Map, req.MaxPlayers, req.Creatures);

            return Ok(new StartResponse
            {
                OpenUrl = $"{_configuration.ServerHost}:{port}",
            });
        }
        catch (Exception ex)
        {
            return Ok(new StartResponse
            {
                Error = ex.Message,
            });
        }
    }

    [HttpPost("kill")]
    [Produces("application/json")]
    public async Task<IActionResult> PostKill(KillRequest req)
    {
        try
        {
            await _serverManager.KillInstance(req.Id);

            return Ok(new KillResponse());
        }
        catch (Exception ex)
        {
            return Ok(new KillResponse
            {
                Error = ex.Message,
            });
        }
    }

    [HttpPost("admin_pw")]
    [Produces("application/json")]
    public async Task<IActionResult> PostAdminPW(KillRequest req)
    {
        try
        {
            return Ok(new AdminPWResponse()
            {
                AdminPW = _serverManager.GetAdminPassword(req.Id)
            });
        }
        catch (InvalidInstanceException)
        {
            return BadRequest();
        }

    }

    [HttpPost("events")]
    [Produces("application/json")]
    public async Task<IActionResult> PostEvents(EventsRequest req)
    {
        return Ok(new EventsResponse()
        {
            Timestamp = req.Timestamp,
            Events = new object[0],
        });
    }
}
