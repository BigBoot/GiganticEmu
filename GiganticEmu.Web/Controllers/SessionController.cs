using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ILogger<SessionController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDatabase _database;


        public SessionController(ILogger<SessionController> logger, UserManager<User> userManager, ApplicationDatabase database)
        {
            _logger = logger;
            _userManager = userManager;
            _database = database;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get([FromHeader(Name = "X-API-Key")] string? token)
        {
            if (token != null)
            {
                var user = await _database.Users
                   .Where(user => user.AuthToken == token)
                   .FirstOrDefaultAsync();

                if (user != null)
                {
                    return Ok(new SessionGetResponse(RequestResult.Success, RequestResult.Success.GetDescription(), user.UserName));
                }
            }

            return Ok(new SessionGetResponse(RequestResult.Unauthorized, RequestResult.Unauthorized.GetDescription()));
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post(SessionPostRequest login)
        {
            var user = await _userManager.FindByNameAsync(login.UserName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, login.Password))
            {
                if (_userManager.SupportsUserLockout && user != null)
                {
                    await _userManager.AccessFailedAsync(user);
                }

                return Ok(new SessionPostResponse(RequestResult.InvalidUser, RequestResult.InvalidUser.GetDescription()));
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return Ok(new SessionPostResponse(RequestResult.AccountLocked, RequestResult.AccountLocked.GetDescription()));
            }

            user.AuthToken = Guid.NewGuid().ToString();
            user.AuthTokenExpires = DateTimeOffset.Now.AddHours(48);
            await _userManager.UpdateAsync(user);

            return Ok(new SessionPostResponse(RequestResult.Success, RequestResult.Success.GetDescription(), user.AuthToken, user.AuthTokenExpires));
        }

        [HttpDelete]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(SessionDeleteRequest logout, [FromHeader(Name = "X-API-Key")] string? token)
        {
            if (token != null)
            {
                var user = await _database.Users
                    .Where(user => user.AuthToken == token)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    user.AuthToken = null;
                    user.AuthTokenExpires = null;

                    await _userManager.UpdateAsync(user);
                }
            }

            return Ok(new SessionDeleteResponse(RequestResult.Success, RequestResult.Success.GetDescription()));
        }
    }
}