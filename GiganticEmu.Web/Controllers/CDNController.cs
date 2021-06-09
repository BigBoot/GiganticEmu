using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CDNController : ControllerBase
    {
        public static readonly string CDN_JSON;
        public static readonly string CDN_JSON_SHA256;
        private readonly ILogger<CDNController> _logger;

        static CDNController()
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

        public CDNController(ILogger<CDNController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult Get()
        {
            return Content(CDN_JSON, "application/json");
        }
    }
}
