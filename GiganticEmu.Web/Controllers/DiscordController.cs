using System;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
using GiganticEmu.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DiscordController : ControllerBase
    {
        private readonly ILogger<FriendsController> _logger;
        private readonly IDbContextFactory<ApplicationDatabase> _databaseFactory;
        private readonly BackendConfiguration _configuration;

        public DiscordController(ILogger<FriendsController> logger, IDbContextFactory<ApplicationDatabase> databaseFactory, IOptions<BackendConfiguration> configuration)
        {
            _logger = logger;
            _databaseFactory = databaseFactory;
            _configuration = configuration.Value;
        }

        [HttpGet]
        public IActionResult Get(string token)
        {
            var redirectUrl = Flurl.Url.Encode(Url.ActionLink(nameof(GetResponse)));
            return Redirect($"https://discord.com/oauth2/authorize?response_type=code&client_id={_configuration.Discord.ClientId}&scope=identify&state={token}&redirect_uri={redirectUrl}&prompt=consent");
        }

        [HttpPost("token")]
        [Produces("application/json")]
        public async Task<IActionResult> PostToken(DiscordPostTokenRequest request, [FromHeader(Name = "X-API-KEY")] string token)
        {
            using var db = _databaseFactory.CreateDbContext();

            var user = await db.Users
                          .Where(user => user.AuthToken == token)
                          .SingleOrDefaultAsync();

            if (user == null)
            {
                return Ok(new DiscordPostTokenResponse(RequestResult.Unauthorized));
            }

            user.DiscordLinkToken = Crypt.CreateSecureRandomString();

            await db.SaveChangesAsync();

            return Ok(new DiscordPostTokenResponse(RequestResult.Success)
            {
                Token = user.DiscordLinkToken,
            });
        }

        [HttpGet("response")]
        public async Task<IActionResult> GetResponse(string code, string state)
        {
            using var db = _databaseFactory.CreateDbContext();

            var user = await db.Users
                          .Where(user => user.DiscordLinkToken == state)
                          .SingleOrDefaultAsync();

            if (user == null)
            {
                return Content("<html><body>Invalid token, please try again!</body></html>", "text/html");
            }

            user.DiscordLinkToken = null;
            await db.SaveChangesAsync();

            var response = await "https://discord.com/api/oauth2/token"
                .WithPolly(policy => policy.RetryAsync(3))
                .PostUrlEncodedAsync(new
                {
                    client_id = _configuration.Discord.ClientId,
                    client_secret = _configuration.Discord.ClientSecret,
                    code = code,
                    redirect_uri = Url.ActionLink(nameof(GetResponse)),
                    grant_type = "authorization_code"
                })
                .ReceiveJson();

            string token = response.access_token.GetString();

            response = await "https://discord.com/api/oauth2/@me"
                .WithOAuthBearerToken(token)
                .WithPolly(policy => policy.RetryAsync(3))
                .GetJsonAsync();

            user.DiscordId = long.Parse(response.user.GetProperty("id").GetString());

            foreach (var oldUser in db.Users.Where(x => x.DiscordId == user.DiscordId && x.Id != user.Id))
            {
                oldUser.DiscordId = null;
                db.Update(oldUser);
            }
            await db.SaveChangesAsync();

            return Content($"<html><body>Done, you can close this window now!</body></html>", "text/html");
        }

        [HttpGet("user")]
        [RequireApiKey]
        public async Task<IActionResult> GetUser(string id)
        {
            using var db = _databaseFactory.CreateDbContext();

            var user = await db.Users
                          .Where(user => user.DiscordId == long.Parse(id))
                          .SingleOrDefaultAsync();

            if (user == null)
            {
                return Ok(new DiscordGetUserResponse(RequestResult.UnknownDiscordId));
            }

            return Ok(new DiscordGetUserResponse(RequestResult.Success)
            {
                Name = user.UserName,
            });
        }
    }
}