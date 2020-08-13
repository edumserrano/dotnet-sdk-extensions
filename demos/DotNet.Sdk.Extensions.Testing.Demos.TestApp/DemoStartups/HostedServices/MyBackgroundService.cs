using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.Demos.TestApp.DemoStartups.HostedServices
{
    public class MyBackgroundService : BackgroundService
    {
        private readonly ICalculator _calculator;

        public MyBackgroundService(ICalculator calculator)
        {
            _calculator = calculator;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _calculator.Sum(1, 1); // implement your logic, this doesn't make sense and is only for demo purposes
                await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
            }
        }
    }
}
