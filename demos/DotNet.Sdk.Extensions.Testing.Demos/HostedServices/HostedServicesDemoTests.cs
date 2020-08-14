using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.Demos.TestApp.DemoStartups.HostedServices;
using DotNet.Sdk.Extensions.Testing.HostedServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Demos.HostedServices
{
    public class HostedServicesDemoTests : IClassFixture<WebApplicationFactory<StartupHostedService>>
    {
        private readonly WebApplicationFactory<StartupHostedService> _webApplicationFactory;

        public HostedServicesDemoTests(WebApplicationFactory<StartupHostedService> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;

            /*
             * The line below is NOT part of the demo. You don't need to do it!
             * It's an artifact of having a single web app to test and wanting to test
             * different Startup classes.
             */
            _webApplicationFactory = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.UseStartup<StartupHostedService>();
                });
        }

        [Fact]
        public async Task HostedServicesRunUntilConditionDemoTest()
        {
            var callCount = 0;
            var calculator = Substitute.For<ICalculator>();
            calculator
                .Sum(Arg.Any<int>(), Arg.Any<int>())
                .Returns(1)
                .AndDoes(info => callCount++);

            await _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<ICalculator>(calculator);
                    });
                })
                .RunUntilAsync(() => callCount == 3);

            callCount.ShouldBeGreaterThanOrEqualTo(3);
        }

        [Fact]
        public async Task HostedServicesRunUntilTimeoutDemoTest()
        {
            var sw = Stopwatch.StartNew();
            await _webApplicationFactory.RunUntilTimeoutAsync(TimeSpan.FromSeconds(2));
            sw.Stop();
            sw.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromSeconds(2));
        }
    }
}
