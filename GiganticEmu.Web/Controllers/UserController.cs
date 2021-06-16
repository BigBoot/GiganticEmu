using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<User> _userManager;

        public UserController(ILogger<UserController> logger, UserManager<User> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post(UserPostRequest register)
        {
            var user = new User()
            {
                Email = register.Email,
                UserName = register.UserName,
            };

            var result = await _userManager.CreateAsync(user, register.Password);
            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code },
                    protocol: Request.Scheme);

                // await _emailSender.SendEmailAsync(register.Email, "Confirm your email",
                //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return Ok(new UserPostResponse(RequestResult.EmailNotConfirmed));
                }
                else
                {
                    user.AuthToken = Guid.NewGuid().ToString();
                    user.AuthTokenExpires = DateTimeOffset.Now.AddHours(48);
                    await _userManager.UpdateAsync(user);

                    return Ok(new UserPostResponse(RequestResult.Success)
                    {
                        AuthToken = user.AuthToken
                    });
                }
            }

            return Ok(new UserPostResponse(RequestResult.RegistrationError)
            {
                Errors = result.Errors.Select(err => new UserPostError { Code = err.Code, Message = err.Description })
            });
        }
    }
}
