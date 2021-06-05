using Microsoft.AspNetCore.Mvc;

namespace HttpClientWithResiliencePolicies.Controllers
{
    [ApiController]
    [Route("/house")]
    public class HomeController : ControllerBase
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
