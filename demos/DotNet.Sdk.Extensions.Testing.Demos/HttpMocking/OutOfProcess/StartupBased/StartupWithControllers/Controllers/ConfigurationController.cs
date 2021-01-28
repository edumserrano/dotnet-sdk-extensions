using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.StartupBased.StartupWithControllers.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IHostEnvironment _hostEnvironment;

        public ConfigurationController(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public string Get()
        {
            return $"hello from {HttpContext.Request.Path} and the mock server environment is {_hostEnvironment.EnvironmentName}";
        }
    }
}