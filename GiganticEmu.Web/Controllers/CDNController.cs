using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CDNController : ControllerBase
    {
        private readonly string _cdnJson;
        private readonly ILogger<CDNController> _logger;


        public CDNController(ILogger<CDNController> logger)
        {
            _logger = logger;

            var assembly = typeof(CDNController).Assembly;

            using (var input = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.cdn.json")!)
            {
                var buffer = new byte[input.Length];
                input.Read(buffer, 0, (int)input.Length);
                _cdnJson = Encoding.UTF8.GetString(buffer);
            }
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult Get()
        {
            return Content(_cdnJson, "application/json");
        }
    }
}
