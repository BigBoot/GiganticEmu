using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]/0.0/arc/auth")]
    public class AuthController : ControllerBase
    {
        private static readonly char[] KEY_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        private readonly ILogger<AuthController> _logger;
        private readonly IDbContextFactory<ApplicationDatabase> _databaseFactory;
        private readonly BackendConfiguration _configuration;

        public AuthController(ILogger<AuthController> logger, IDbContextFactory<ApplicationDatabase> databaseFactory, IOptions<BackendConfiguration> configuration)
        {
            _logger = logger;
            _databaseFactory = databaseFactory;
            _configuration = configuration.Value;
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromHeader(Name = "Host")] string host, [FromForm(Name = "arc_token")] string token, [FromForm(Name = "v")] int version)
        {
            using var db = _databaseFactory.CreateDbContext();

            var user = await db.Users.Where(user => user.AuthToken == token).FirstOrDefaultAsync();

            if (user != null)
            {
                user.SalsaSCK = GenerateKey(16);

                await db.SaveChangesAsync();
                IPAddress? miceIp;
                if (!IPAddress.TryParse(_configuration.MiceHost, out miceIp))
                {
                    miceIp = (await Dns.GetHostAddressesAsync(_configuration.MiceHost))
                        .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                        .FirstOrDefault();
                }

                return Content(new
                {
                    result = "ok",
                    auth = token,
                    token = token,
                    name = user.UserName,
                    username = user.Email,
                    buddy_key = false,
                    founders_pack = true,

                    host = miceIp?.ToString(),
                    port = _configuration.MicePort,
                    ck = Convert.ToBase64String(new byte[] { 0, 0 }.Concat(Encoding.ASCII.GetBytes(_configuration.SalsaCK)).ToArray()),
                    sck = Convert.ToBase64String(new byte[] { 0, 0 }.Concat(Encoding.ASCII.GetBytes(user.SalsaSCK)).ToArray()),

                    flags = "", // unknown string
                    xbox_preview = false,
                    accounts = "accmple", // unknown string
                    mostash_verbosity_level = 0,
                    min_version = version,
                    current_version = version,
                    catalog = new
                    {
                        cdn_url = $"{HttpContext.Request.Scheme}://{host}/cdn",
                        sha256_digest = CDNController.CDN_JSON_SHA256,
                    },
                    announcements = new
                    {
                        message = "serverMessage",
                        status = "serverStatus",
                    },
                    voice_chat = new
                    {
                        baseurl = "http://127.0.01/voice.html",
                        username = "sup:.username.@voice.sipServ.com",
                        token = "sipToken"
                    },
                }.ToJson(), "application/json");
            }
            return Unauthorized();
        }

        private static string GenerateKey(int length)
        {
            byte[] data = new byte[4 * length];
            RandomNumberGenerator.Fill(data);

            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i += 2)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % KEY_CHARS.Length;

                result.Append(KEY_CHARS[idx]);
                result.Append(KEY_CHARS[idx]);
            }

            return result.ToString();
        }
    }
}
