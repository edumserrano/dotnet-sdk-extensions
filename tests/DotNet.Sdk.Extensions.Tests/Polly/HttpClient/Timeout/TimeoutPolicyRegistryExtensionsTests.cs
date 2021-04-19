using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly;
using DotNet.Sdk.Extensions.Polly.HttpClient.Timeout;
using DotNet.Sdk.Extensions.Polly.HttpClient.Timeout.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Timeout.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Polly.Registry;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Timeout
{
    /// <summary>
    /// Tests for the <see cref="TimeoutPolicyRegistryExtensions"/> class
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class TimeoutPolicyRegistryExtensionsTests
    {
        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyRegistryExtensions.AddHttpClientTimeoutPolicy(IPolicyRegistry{string},string,string,IServiceProvider)"/>
        /// overload method adds the timeout policy to the Polly registry
        /// </summary>
        [Fact]
        public void AddHttpClientTimeout()
        {
            var policyKey = "testPolicy";
            var optionsName = "timeoutOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = 1);
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, optionsName, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyRegistryExtensions.AddHttpClientTimeoutPolicy{TPolicyConfiguration}(IPolicyRegistry{string},string,string,IServiceProvider)"/>
        /// overload method adds the timeout policy to the Polly registry
        /// </summary>
        [Fact]
        public void AddHttpClientTimeout2()
        {
            var policyKey = "testPolicy";
            var optionsName = "timeoutOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = 1);
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientTimeoutPolicy<TestTimeoutPolicyConfiguration>(policyKey, optionsName, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyRegistryExtensions.AddHttpClientTimeoutPolicy(IPolicyRegistry{string},string,string,ITimeoutPolicyConfiguration,IServiceProvider)"/>
        /// overload method adds the timeout policy to the Polly registry
        /// </summary>
        [Fact]
        public void AddHttpClientTimeoutPolicy3()
        {
            var policyKey = "testPolicy";
            var optionsName = "timeoutOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = 1);
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var policyConfiguration = Substitute.For<ITimeoutPolicyConfiguration>();
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, optionsName, policyConfiguration, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyRegistryExtensions.AddHttpClientTimeoutPolicy(IPolicyRegistry{string},string,TimeoutOptions,ITimeoutPolicyConfiguration)"/>
        /// overload method adds the timeout policy to the Polly registry
        /// </summary>
        [Fact]
        public void AddHttpClientTimeoutPolicy4()
        {
            var policyKey = "testPolicy";
            var services = new ServiceCollection();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var policyConfiguration = Substitute.For<ITimeoutPolicyConfiguration>();
                var options = new TimeoutOptions { TimeoutInSecs = 1 };
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, options, policyConfiguration);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        /// <summary>
        /// Tests that the IPolicyRegistry.AddHttpClientTimeoutPolicy method configures the timeout policy
        /// with the TimeoutOptions.TimeoutInSecs.
        ///
        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
        /// to test that the policy's configuration is as expected.
        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
        /// seem to be possible.
        /// </summary>
        [Fact]
        public async Task AddHttpClientTimeoutPolicyHonorsOptions()
        {
            var policyKey = "testPolicy";
            var timeoutInSecs = 1;
            var services = new ServiceCollection();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var policyConfiguration = Substitute.For<ITimeoutPolicyConfiguration>();
                var options = new TimeoutOptions { TimeoutInSecs = timeoutInSecs };
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, options, policyConfiguration);
            });

            var timeoutPolicy = services.GetHttpPolicy<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey);
            var policyResult = await timeoutPolicy.ExecuteAndCaptureWithForcedTimeout(timeoutInSecs + 1);
            policyResult.FinalException.ShouldBeOfType<TimeoutRejectedException>();
        }

        /// <summary>
        /// Tests that the IPolicyRegistry.AddHttpClientTimeoutPolicy method will trigger the policy configuration
        /// when it times out.
        /// </summary>
        [Fact]
        public async Task AddHttpClientTimeoutPolicyTriggersPolicyConfigurationOnTimeout()
        {
            var policyKey = "testPolicy";
            var timeoutInSecs = 1;
            var services = new ServiceCollection();
            var timeoutPolicyConfiguration = Substitute.For<ITimeoutPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var options = new TimeoutOptions { TimeoutInSecs = timeoutInSecs };
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, options, timeoutPolicyConfiguration);
            });

            var timeoutPolicy = services.GetHttpPolicy<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey);
            await timeoutPolicy.ExecuteAndCaptureWithForcedTimeout(timeoutInSecs + 1);
            await timeoutPolicyConfiguration
                .ReceivedWithAnyArgs(1)
                .OnTimeoutASync(
                    timeoutOptions: default!,
                    context: default!,
                    requestTimeout: default,
                    timedOutTask: default!,
                    exception: default!);
        }

        /// <summary>
        /// Tests that the IPolicyRegistry.AddHttpClientTimeoutPolicy method will trigger the policy configuration
        /// when it times out and that the call to ITimeoutPolicyConfiguration.OnTimeoutASync is done with the
        /// expected values.
        /// </summary>
        [Fact]
        public async Task AddHttpClientTimeoutPolicyTriggersPolicyConfigurationWithExpectedValuesOnTimeout()
        {
            TimeoutOptions timeoutOptions = null!;
            var requestTimeout = TimeSpan.Zero;
            var timeoutPolicyConfiguration = Substitute.For<ITimeoutPolicyConfiguration>();
            timeoutPolicyConfiguration
                .WhenForAnyArgs(x =>
                    x.OnTimeoutASync(
                        timeoutOptions: default!,
                        context: default!,
                        requestTimeout: default,
                        timedOutTask: default!,
                        exception: default!))
                .Do(callInfo =>
                {
                    timeoutOptions = callInfo.ArgAt<TimeoutOptions>(0);
                    requestTimeout = callInfo.ArgAt<TimeSpan>(2);
                });
            var policyKey = "testPolicy";
            var timeoutInSecs = 1;
            var services = new ServiceCollection();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var options = new TimeoutOptions { TimeoutInSecs = timeoutInSecs };
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, options, timeoutPolicyConfiguration);
            });
            
            var timeoutPolicy = services.GetHttpPolicy<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey);
            await timeoutPolicy.ExecuteAndCaptureWithForcedTimeout(timeoutInSecs + 1);

            // Only asserting these two properties. Should I try to assert more?
            timeoutOptions.TimeoutInSecs.ShouldBe(timeoutInSecs);
            requestTimeout.ShouldBe(TimeSpan.FromSeconds(timeoutInSecs));
        }
    }
}
