using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
using GiganticEmu.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace Web.Controllers;

[Controller]
[Route("[controller]")]
public class RegisterController : Controller
{
    private const string SESSION_KEY_DISCORD_BEARER_TOKEN = "DISCORD_BEARER_TOKEN";
    private const string SESSION_KEY_DISCORD_STATE = "DISCORD_STATE";

    private readonly ILogger<RegisterController> _logger;
    private readonly IDbContextFactory<ApplicationDatabase> _databaseFactory;
    private readonly BackendConfiguration _configuration;
    private readonly UserManager<User> _userManager;

    public RegisterController(ILogger<RegisterController> logger, UserManager<User> userManager, IDbContextFactory<ApplicationDatabase> databaseFactory, IOptions<BackendConfiguration> configuration)
    {
        _logger = logger;
        _userManager = userManager;
        _databaseFactory = databaseFactory;
        _configuration = configuration.Value;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return View("Discord");
    }

    [HttpPost("discord")]
    public IActionResult PostDiscord()
    {
        var token = Crypt.CreateSecureRandomString();
        HttpContext.Session.SetString(SESSION_KEY_DISCORD_STATE, token);
        var redirectUrl = Flurl.Url.Encode(Url.ActionLink(nameof(GetDiscord)));
        return Redirect($"https://discord.com/oauth2/authorize?response_type=code&client_id={_configuration.Discord.ClientId}&scope=identify&state={token}&redirect_uri={redirectUrl}&prompt=consent");
    }

    [HttpGet("discord")]
    public async Task<IActionResult> GetDiscord(string code, string state)
    {
        if (HttpContext.Session.GetString(SESSION_KEY_DISCORD_STATE) != state)
        {
            ModelState.AddModelError("", "Invalid state, please try again!");
            return View("Discord");
        }

        try
        {
            var response = await "https://discord.com/api/oauth2/token"
                .WithPolly(policy => policy.RetryAsync(3))
                .PostUrlEncodedAsync(new
                {
                    client_id = _configuration.Discord.ClientId,
                    client_secret = _configuration.Discord.ClientSecret,
                    code = code,
                    redirect_uri = Url.ActionLink(nameof(GetDiscord)),
                    grant_type = "authorization_code"
                })
                .ReceiveJson();

            string bearerToken = response.access_token.GetString();
            HttpContext.Session.SetString(SESSION_KEY_DISCORD_BEARER_TOKEN, bearerToken);
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Failed to verify discord account, please try again!");
            return View("Discord");
        }

        return View("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(RegisterUser register)
    {
        if (ModelState.IsValid)
        {
            long discordId;
            try
            {
                var response = await "https://discord.com/api/oauth2/@me"
                    .WithOAuthBearerToken(HttpContext.Session.GetString(SESSION_KEY_DISCORD_BEARER_TOKEN))
                    .WithPolly(policy => policy.RetryAsync(3))
                    .GetJsonAsync();

                discordId = long.Parse(response.user.GetProperty("id").GetString());
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Failed to get Discord user info, please try again!");
                return View("Discord");
            }

            var user = new User()
            {
                Email = register.Email,
                UserName = register.UserName,
                DiscordId = discordId,
            };

            var result = await _userManager.CreateAsync(user, register.Pwd);
            if (result.Succeeded)
            {
                var db = await _databaseFactory.CreateDbContextAsync();
                foreach (var oldUser in db.Users.Where(x => x.DiscordId == discordId && x.Id != user.Id))
                {
                    oldUser.DiscordId = null;
                }
                await db.SaveChangesAsync();

                return View("Done");
            }
            AddErrors(result);
        }

        return View("Index");
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            switch (error.Code)
            {
                case nameof(IdentityErrorDescriber.DuplicateEmail):
                case nameof(IdentityErrorDescriber.InvalidEmail):
                    ModelState.AddModelError(nameof(RegisterUser.Email), error.Description);
                    break;

                case nameof(IdentityErrorDescriber.DuplicateUserName):
                case nameof(IdentityErrorDescriber.InvalidUserName):
                    ModelState.AddModelError(nameof(RegisterUser.UserName), error.Description);
                    break;

                case nameof(IdentityErrorDescriber.PasswordRequiresDigit):
                case nameof(IdentityErrorDescriber.PasswordRequiresLower):
                case nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric):
                case nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars):
                case nameof(IdentityErrorDescriber.PasswordRequiresUpper):
                case nameof(IdentityErrorDescriber.PasswordTooShort):
                    ModelState.AddModelError(nameof(RegisterUser.Pwd), error.Description);
                    ModelState.AddModelError(nameof(RegisterUser.ConfirmPwd), error.Description);
                    break;

                case nameof(IdentityErrorDescriber.PasswordMismatch):
                    ModelState.AddModelError(nameof(RegisterUser.ConfirmPwd), error.Description);
                    break;

                default:
                    ModelState.AddModelError("", error.Description);
                    break;
            }

        }
    }
}