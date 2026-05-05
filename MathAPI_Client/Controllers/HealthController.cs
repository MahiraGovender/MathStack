using Microsoft.AspNetCore.Mvc;

namespace MathAPI_Client.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet("/health")]
        public IActionResult Index()
        {
            return Content("ok");
        }
    }
}