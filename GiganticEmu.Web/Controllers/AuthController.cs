using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Fetchgoods.Text.Json.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]/0.0/arc/auth")]
    public class AuthController : ControllerBase
    {
        public static readonly string CDN_JSON;
        public static readonly string CDN_JSON_SHA256;
        private readonly ILogger<AuthController> _logger;

        static AuthController()
        {
            var assembly = Assembly.GetEntryAssembly()!;

            using (var input = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.cdn.json")!)
            {
                var buffer = new byte[input.Length];
                input.Read(buffer, 0, (int)input.Length);
                CDN_JSON = Encoding.UTF8.GetString(buffer);

                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] sha = sha256Hash.ComputeHash(buffer);
                    CDN_JSON_SHA256 = string.Join(" ", sha.Select(b => b.ToString("x2")));
                }
            }
        }

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Produces("application/json")]
        public IActionResult Post()
        {
            var version = 16897;
            var token = "zwl42ixhzshhfajvt8likv8ujkyoxlrn";
            var http_host = "127.0.0.1";
            var http_port = 3000;
            var mice_host = "127.0.0.1";
            var mice_port = 4000;
            var SALSA_CK = "aaaaaaaaaaaaaaaa";
            var SALSA_SCK = "bbbbbbbbbbbbbbbb";

            return Content(new
            {
                result = "ok",
                auth = token,
                token = token,
                name = "Player",
                username = "test@example.de",
                buddy_key = false,
                founders_pack = true,

                host = mice_host,
                port = mice_port,
                ck = Convert.ToBase64String(new byte[] { 0, 0 }.Concat(Encoding.UTF8.GetBytes(SALSA_CK)).ToArray()),
                sck = Convert.ToBase64String(new byte[] { 0, 0 }.Concat(Encoding.UTF8.GetBytes(SALSA_SCK)).ToArray()),

                flags = "", // unknown string
                xbox_preview = false,
                accounts = "accmple", // unknown string
                mostash_verbosity_level = 0,
                min_version = version,
                current_version = version, // game doesnt event check
                catalog = new
                {

                    cdn_url = $"http://{http_host}:{http_port}/cdn",
                    sha256_digest = "53e8adf03a506dedde073150537b9bd79168ae7e30b3b2152835c22abf98455b",
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
    }
}
