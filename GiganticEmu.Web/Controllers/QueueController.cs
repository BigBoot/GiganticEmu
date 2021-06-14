using System.Linq;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly ILogger<QueueController> _logger;
        private readonly ApplicationDatabase _database;
        private readonly BackendConfiguration _configuration;


        public QueueController(ILogger<QueueController> logger, ApplicationDatabase database, IOptions<BackendConfiguration> configuration)
        {
            _logger = logger;
            _database = database;
            _configuration = configuration.Value;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get([FromHeader(Name = "X-API-Key")] string? token)
        {
            if (_configuration.ApiKey != null && token == _configuration.ApiKey)
            {
                var players = await _database.Users
                    .Where(user => user.InQueue == true)
                    .Select(user => user.UserName)
                    .ToListAsync();

                return Ok(new QueueGetResponse(RequestResult.Success)
                {
                    Players = players
                });
            }

            return Ok(new QueueGetResponse(RequestResult.Unauthorized));
        }
    }
}