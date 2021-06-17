using System;
using System.Linq;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
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
        private readonly IDbContextFactory<ApplicationDatabase> _databaseFactory;


        public SessionController(ILogger<SessionController> logger, UserManager<User> userManager, IDbContextFactory<ApplicationDatabase> databaseFactory)
        {
            _logger = logger;
            _userManager = userManager;
            _databaseFactory = databaseFactory;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get([FromHeader(Name = "X-API-Key")] string? token)
        {
            if (token != null)
            {
                var db = _databaseFactory.CreateDbContext();

                var user = await db.Users
                   .Where(user => user.AuthToken == token)
                   .FirstOrDefaultAsync();

                if (user != null)
                {
                    return Ok(new SessionGetResponse(RequestResult.Success)
                    {
                        UserName = user.UserName
                    });
                }
            }

            return Ok(new SessionGetResponse(RequestResult.Unauthorized));
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

                return Ok(new SessionPostResponse(RequestResult.InvalidUser));
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return Ok(new SessionPostResponse(RequestResult.AccountLocked));
            }

            user.AuthToken = Guid.NewGuid().ToString();
            user.AuthTokenExpires = DateTimeOffset.Now.AddHours(48);
            await _userManager.UpdateAsync(user);

            return Ok(new SessionPostResponse(RequestResult.Success)
            {
                AuthToken = user.AuthToken,
                Expiry = user.AuthTokenExpires
            });
        }

        [HttpDelete]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(SessionDeleteRequest logout, [FromHeader(Name = "X-API-Key")] string? token)
        {
            if (token != null)
            {
                var db = _databaseFactory.CreateDbContext();

                var user = await db.Users
                    .Where(user => user.AuthToken == token)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    user.AuthToken = null;
                    user.AuthTokenExpires = null;

                    await _userManager.UpdateAsync(user);
                }
            }

            return Ok(new SessionDeleteResponse(RequestResult.Success));
        }
    }
}