using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleAppWithGenericHost
{
    public class MyApp
    {
        private readonly ILogger<MyApp> _logger;
        private readonly MyOptions _myOptions;

        public MyApp(IOptions<MyOptions> myOptions, ILogger<MyApp> logger)
        {
            _logger = logger;
            _myOptions = myOptions.Value;
        }

        public void Run()
        {
            _logger.LogInformation("log message from logger!");
            _logger.LogInformation($"This configuration value came from the appsettings: SomeOption={_myOptions.SomeOption}");
            Console.WriteLine("Press enter key to terminate the app");
            Console.ReadLine();
        }
    }
}
