using System.Linq;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
using GiganticEmu.Web;
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
        private readonly IDbContextFactory<ApplicationDatabase> _databaseFactory;
        private readonly BackendConfiguration _configuration;


        public QueueController(ILogger<QueueController> logger, IDbContextFactory<ApplicationDatabase> databaseFactory, IOptions<BackendConfiguration> configuration)
        {
            _logger = logger;
            _databaseFactory = databaseFactory;
            _configuration = configuration.Value;
        }

        [HttpGet]
        [RequireApiKey]
        [Produces("application/json")]
        public async Task<IActionResult> Get([FromHeader(Name = "X-API-Key")] string? token)
        {
            if (_configuration.ApiKey != null && token == _configuration.ApiKey)
            {
                using var db = _databaseFactory.CreateDbContext();

                var players = await db.Users
                    .Where(user => user.InQueue == true)
                    .Select(user => user.UserName)
                    .OfType<string>()
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