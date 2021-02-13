using Microsoft.AspNetCore.Mvc;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.StartupBased.StartupWithControllers.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return $"hello from {HttpContext.Request.Path}";
        }
    }
}
