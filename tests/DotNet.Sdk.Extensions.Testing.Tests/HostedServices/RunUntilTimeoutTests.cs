using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.Configuration;
using DotNet.Sdk.Extensions.Testing.HostedServices;
using DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliary;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices
{
    /// <summary>
    /// These tests simulate an app with a <see cref="BackgroundService"/>.
    /// For more info see <seealso cref="StartupHostedService"/> and <seealso cref="HostedServicesWebApplicationFactory"/>
    /// </summary>
    [Trait("Category", XUnitCategories.HostedServices)]
    public class RunUntilTimeoutTests : IClassFixture<HostedServicesWebApplicationFactory>
    {
        /// <summary>
        /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilTimeoutAsync{T}(WebApplicationFactory{T},TimeSpan)"/>
        /// extension method.
        /// </summary>
        [Fact]
        public void WebApplicationFactoryRunUntilTimeoutValidatesArguments()
        {
            var webApplicationFactoryArgumentNullException = Should.Throw<ArgumentNullException>(() =>
            {
                RunUntilExtensions.RunUntilTimeoutAsync<StartupHostedService>(webApplicationFactory: null!, TimeSpan.FromSeconds(1));
            });
            webApplicationFactoryArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'webApplicationFactory')");
        }

        /// <summary>
        /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilTimeoutAsync(IHost,TimeSpan)"/>
        /// extension method.
        /// </summary>
        [Fact]
        public void HostRunUntilTimeoutValidatesArguments()
        {
            var hostArgumentNullException = Should.Throw<ArgumentNullException>(() =>
            {
                RunUntilExtensions.RunUntilTimeoutAsync(host: null!, TimeSpan.FromSeconds(1));
            });
            hostArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'host')");
        }

        /// <summary>
        /// Tests that <seealso cref="RunUntilExtensions.RunUntilTimeoutAsync{T}(WebApplicationFactory{T},TimeSpan)"/>
        /// terminates the Host created by the WebApplicationFactory after the specified timeout.
        /// Furthermore the <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 500 ms so
        /// we should also have at least 3 calls to that method. The 4th call may or may not happen.
        /// </summary>
        [Fact]
        public async Task WebApplicationFactoryRunUntilTimeout()
        {
            var callCount = 0;
            var calculator = Substitute.For<ICalculator>();
            calculator
                .Sum(Arg.Any<int>(), Arg.Any<int>())
                .Returns(1)
                .AndDoes(info =>
                {
                    ++callCount;
                });

            using var webApplicationFactory = new HostedServicesWebApplicationFactory();
            var sw = Stopwatch.StartNew();
            await webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder
                        .ConfigureTestServices(services =>
                        {
                            services.AddSingleton(calculator);
                        });
                })
                .RunUntilTimeoutAsync(TimeSpan.FromSeconds(2));
            sw.Stop();

            sw.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(1900));
            callCount.ShouldBeGreaterThanOrEqualTo(3);
        }

        /// <summary>
        /// Tests that <seealso cref="RunUntilExtensions.RunUntilTimeoutAsync(IHost,TimeSpan)"/>
        /// terminates the Host after the specified timeout.
        /// Furthermore the <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 500 ms so
        /// we should also have at least 3 calls to that method. The 4th call may or may not happen.
        /// </summary>
        [Fact]
        public async Task HostRunUntilTimeout()
        {
            var callCount = 0;
            var calculator = Substitute.For<ICalculator>();
            calculator
                .Sum(Arg.Any<int>(), Arg.Any<int>())
                .Returns(1)
                .AndDoes(info =>
                {
                    ++callCount;
                });

            // This code creating the Host would exist somewhere in app being tested.
            // In a real scenario we would call the function that creates the Host.
            // For this test we incorporate the host creation in this test. 
            var hostBuilder = Host
                .CreateDefaultBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ICalculator, Calculator>();
                    services.AddHostedService<MyBackgroundService>();
                });

            // This is for overriding services for test purposes.
            using var host = hostBuilder
                .ConfigureServices((hostContext, services) =>
                 {
                     services.AddSingleton(calculator);
                 })
                .Build();

            var sw = Stopwatch.StartNew();
            await host.RunUntilTimeoutAsync(TimeSpan.FromSeconds(2));
            sw.Stop();

            sw.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(1900));
            callCount.ShouldBeGreaterThanOrEqualTo(3);
        }
    }
}
