using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HostedServices;
using DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliar;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices
{
    /// <summary>
    /// These tests simulate an app with a <see cref="BackgroundService"/>.
    /// For more info see <seealso cref="StartupHostedService"/> and <seealso cref="HostedServicesWebApplicationFactory"/>
    /// </summary>
    public class WebApplicationFactoryRunUntilWithAsyncPredicateTests
    {
        public static TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicateAsync, Type, string> AsyncPredicateValidateArgumentsData =>
            new TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicateAsync, Type, string>
            {
                { null!, ()=> Task.FromResult(true), typeof(ArgumentNullException), "Value cannot be null. (Parameter 'webApplicationFactory')" },
                { new HostedServicesWebApplicationFactory(), null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'predicateAsync')" },
            };

        /// <summary>
        /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicateAsync)"/>
        /// extension method.
        /// </summary>
        [Theory]
        [MemberData(nameof(AsyncPredicateValidateArgumentsData))]
        public void RunUntilAsyncPredicateValidatesArguments(
            HostedServicesWebApplicationFactory webApplicationFactory,
            RunUntilPredicateAsync predicateAsync,
            Type exceptionType,
            string exceptionMessage)
        {
            var exception = Should.Throw(
                actual: () => RunUntilExtensions.RunUntilAsync(webApplicationFactory, predicateAsync),
                exceptionType: exceptionType);
            exception.Message.ShouldBe(exceptionMessage);
        }

        public static TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicateAsync, Action<RunUntilOptions>, Type, string> SyncPredicateWithOptionsValidateArgumentsData =>
            new TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicateAsync, Action<RunUntilOptions>, Type, string>
            {
                { null!, ()=>Task.FromResult(true), options => {} , typeof(ArgumentNullException), "Value cannot be null. (Parameter 'webApplicationFactory')" },
                { new HostedServicesWebApplicationFactory(), null!, options => {}, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'predicateAsync')" },
                { new HostedServicesWebApplicationFactory(), ()=>Task.FromResult(true), null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'configureOptions')" },
            };

        /// <summary>
        /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicateAsync,Action{RunUntilOptions})"/>
        /// extension method.
        /// </summary>
        [Theory]
        [MemberData(nameof(SyncPredicateWithOptionsValidateArgumentsData))]
        public void RunUntilAsyncPredicateWithOptionsValidatesArguments(
            HostedServicesWebApplicationFactory webApplicationFactory,
            RunUntilPredicateAsync predicateAsync,
            Action<RunUntilOptions> configureOptions,
            Type exceptionType,
            string exceptionMessage)
        {
            var exception = Should.Throw(
                actual: () => RunUntilExtensions.RunUntilAsync(webApplicationFactory, predicateAsync, configureOptions),
                exceptionType: exceptionType);
            exception.Message.ShouldBe(exceptionMessage);
        }

        /// <summary>
        /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicateAsync)"/>
        /// terminates the Host after the predicate is met.
        /// The <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 50 ms and the default
        /// <seealso cref="RunUntilOptions.Timeout"/> is 5 seconds so the predicate should be met before the timeout.
        /// </summary>
        [Fact]
        public async Task RunUntilAsyncPredicate()
        {
            var callCount = 0;
            var calculator = Substitute.For<ICalculator>();
            calculator
                .Sum(Arg.Any<int>(), Arg.Any<int>())
                .Returns(1)
                .AndDoes(info => callCount++);

            using var webApplicationFactory = new HostedServicesWebApplicationFactory();
            await webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton(calculator);
                    });
                })
                .RunUntilAsync(() => Task.FromResult(callCount == 3));

            callCount.ShouldBeGreaterThanOrEqualTo(3);
        }

        /// <summary>
        /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicateAsync,Action{RunUntilOptions})"/>
        /// times out if the predicate is not met within the configured timeout.
        /// The <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 50 ms so if we set the timeout to 100 ms
        /// and the predicate to stop the Host after receiveing 4 calls then the timeout should be triggered before the predicate is met.
        /// </summary>
        [Fact]
        public async Task RunUntilAsyncPredicateTimeoutOption()
        {
            var callCount = 0;
            var calculator = Substitute.For<ICalculator>();
            calculator
                .Sum(Arg.Any<int>(), Arg.Any<int>())
                .Returns(1)
                .AndDoes(info => callCount++);

            using var webApplicationFactory = new HostedServicesWebApplicationFactory()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton(calculator);
                    });
                });

            var runUntilTask = webApplicationFactory.RunUntilAsync(() => Task.FromResult(callCount == 4), options => options.Timeout = TimeSpan.FromMilliseconds(100));
            var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
            exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:00.1000000. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you inteded, if you want to run the Host for a set period of time consider using RunUntilExtensions.RunUntilTimeoutAsync instead");
        }

        /// <summary>
        /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicateAsync,Action{RunUntilOptions})"/>
        /// checks the predicate using the <seealso cref="RunUntilOptions.PredicateCheckInterval"/> value.
        /// This test sets up the PredicateCheckInterval and Timeout options values so that the timeout occurs even before the first check is made.
        /// The <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 50 ms so if we set the timeout to 100 ms
        /// and the predicate to stop the Host after receiveing 1 call then the timeout should NOT be triggered before the predicate is met.
        /// However, the timeout is indeed triggered before the predicate is met because this test sets up the PredicateCheckInterval and Timeout options values
        /// so that the timeout occurs even before the first check is made.
        /// </summary>
        [Fact]
        public async Task RunUntilAsyncPredicatePredicateCheckIntervalOption()
        {
            var callCount = 0;
            var calculator = Substitute.For<ICalculator>();
            calculator
                .Sum(Arg.Any<int>(), Arg.Any<int>())
                .Returns(1)
                .AndDoes(info => callCount++);

            using var webApplicationFactory = new HostedServicesWebApplicationFactory()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton(calculator);
                    });
                });

            var runUntilTask = webApplicationFactory.RunUntilAsync(() => Task.FromResult(callCount == 1), options =>
            {
                options.PredicateCheckInterval = TimeSpan.FromMilliseconds(100);
                options.Timeout = TimeSpan.FromMilliseconds(50);
            });
            var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
            exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:00.0500000. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you inteded, if you want to run the Host for a set period of time consider using RunUntilExtensions.RunUntilTimeoutAsync instead");
            callCount.ShouldBeGreaterThanOrEqualTo(1); // this is true which means the RunUntilAsync predicate was met however it wasn't checked before the timeout was triggered
        }
    }
}
