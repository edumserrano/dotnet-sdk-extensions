using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleAppWithGenericHost2
{
    public class MyApp
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<MyApp> _logger;
        private readonly MyOptions _myOptions;

        public MyApp(
            IHostApplicationLifetime hostApplicationLifetime,
            IOptions<MyOptions> myOptions, 
            ILogger<MyApp> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
            _myOptions = myOptions.Value;
        }

        public void Run()
        {
            _logger.LogInformation("log message from logger!");
            _logger.LogInformation($"This configuration value came from the appsettings: SomeOption={_myOptions.SomeOption}");
            Console.WriteLine("Press enter key to terminate the app");
            Console.ReadLine();
            _hostApplicationLifetime.StopApplication();
        }
    }
}
