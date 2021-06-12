using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            var assembly = typeof(CDNController).Assembly;

            CDN_JSON = System.IO.File.ReadAllText("Resources/cdn.json");

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] sha = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(CDN_JSON));
                CDN_JSON_SHA256 = string.Join(" ", sha.Select(b => b.ToString("x2")));
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
