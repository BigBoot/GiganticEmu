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
using FluentEmail.Core;
using Microsoft.Extensions.Options;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IFluentEmail _email;
        private readonly BackendConfiguration _configuration;

        public UserController(ILogger<UserController> logger, UserManager<User> userManager, IFluentEmail email, IOptions<BackendConfiguration> configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _email = email;
            _configuration = configuration.Value;
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

        [HttpDelete("pw")]
        [Produces("application/json")]
        public async Task<IActionResult> UserPWDelete(UserPWDeleteRequest resetPW)
        {
            var user = await _userManager.FindByNameAsync(resetPW.UserName);

            if (user == null)
            {
                return Ok(new UserPWDeleteResponse(RequestResult.Success));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);


            var result = await _email
                .To(user.Email)
                .Subject("Password reset request for your GigantEmu account")
                .Body(
                    $"Hi {user.UserName},\n\n" +
                    $"You recently requested to reset the password for your GiganticEmu account. Please use the following code to confirm the change:\n\n" +
                    $"<pre>{token}</pre>\n\n" +
                    $"If you didn't make this request, then you can ignore this email.\n\n",
                true)
                .SendAsync();

            if (!result.Successful)
            {
                return Ok(new UserPWDeleteResponse(RequestResult.FailedToSendEmail));
            }

            return Ok(new UserPWDeleteResponse(RequestResult.Success));
        }

        [HttpPost("pw")]
        [Produces("application/json")]
        public async Task<IActionResult> UserPWPost(UserPWPostRequest changePW)
        {
            var user = await _userManager.FindByNameAsync(changePW.UserName);

            if (user == null)
            {
                return Ok(new UserPWPostResponse(RequestResult.InvalidToken));
            }

            var result = await _userManager.ResetPasswordAsync(user, changePW.Token, changePW.NewPassword);

            if (result.Succeeded)
            {
                return Ok(new UserPWPostResponse(RequestResult.Success));
            }

            return Ok(new UserPWPostResponse(RequestResult.ResetPWError)
            {
                Errors = result.Errors.Select(err => new UserPostError { Code = err.Code, Message = err.Description })
            });
        }
    }
}
