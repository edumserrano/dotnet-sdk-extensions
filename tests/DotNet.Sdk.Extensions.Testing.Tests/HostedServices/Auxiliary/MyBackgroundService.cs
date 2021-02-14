using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliary
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
                await Task.Yield(); // helps out with the tests asserting this was called X number of times. Without this some RunUntilTimeoutAsync tests don't work as expected in linux
                _calculator.Sum(1, 1); // implement your logic, this doesn't make sense and is only for demo purposes
                await Task.Yield(); // helps out with the tests asserting this was called X number of times. Without this some RunUntilTimeoutAsync tests don't work as expected in linux
                await Task.Delay(TimeSpan.FromMilliseconds(50), stoppingToken);
            }
        }
    }
}
