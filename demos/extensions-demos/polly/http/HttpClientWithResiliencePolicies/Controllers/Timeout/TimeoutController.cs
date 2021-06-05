using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Polly.Timeout;

namespace HttpClientWithResiliencePolicies.Controllers.Timeout
{
    [ApiController]
    [Route("/timeout")]
    public class TimeoutController : ControllerBase
    {
        private readonly HttpClientWithTimeout _httpClientWithTimeout;

        public TimeoutController(HttpClientWithTimeout httpClientWithTimeout)
        {
            _httpClientWithTimeout = httpClientWithTimeout;
        }

        public async Task<IActionResult> TestTimeout()
        {
            await _httpClientWithTimeout.DoSomeHttpOperationAsync();
            return Ok();
        }
    }
}
