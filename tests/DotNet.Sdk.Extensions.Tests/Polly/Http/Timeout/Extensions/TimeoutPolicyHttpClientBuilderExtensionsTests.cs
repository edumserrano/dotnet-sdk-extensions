using System.Linq;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary.Polly;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Extensions
{
    /// <summary>
    /// Tests for the <see cref="TimeoutPolicyHttpClientBuilderExtensions"/> class
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class TimeoutPolicyHttpClientBuilderExtensionsTests
    {
        [Fact]
        public void AddTimeoutPolicyInlineOptionsValidation()
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = -1;
                });

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"DataAnnotation validation failed for members: 'TimeoutInSecs' with the error: 'The field TimeoutInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
        }

        [Fact]
        public void AddTimeoutPolicy1()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldBeConfiguredAsExpected(timeoutInSecs);
            timeoutPolicy.ShouldTriggerPolicyConfiguration(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyConfigurationType: typeof(DefaultTimeoutPolicyConfiguration));
        }

        [Fact]
        public void AddTimeoutPolicy2()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 1;
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutInSecs);
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldBeConfiguredAsExpected(timeoutInSecs);
            timeoutPolicy.ShouldTriggerPolicyConfiguration(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyConfigurationType: typeof(DefaultTimeoutPolicyConfiguration));
        }

        [Fact]
        public void AddTimeoutPolicy3()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy<TestTimeoutPolicyConfiguration>(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldBeConfiguredAsExpected(timeoutInSecs);
            timeoutPolicy.ShouldTriggerPolicyConfiguration(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyConfigurationType: typeof(TestTimeoutPolicyConfiguration));
        }

        [Fact]
        public void AddTimeoutPolicy4()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 1;
            var optionsName = "GitHubOptions";

            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutInSecs);
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy<TestTimeoutPolicyConfiguration>(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldBeConfiguredAsExpected(timeoutInSecs);
            timeoutPolicy.ShouldTriggerPolicyConfiguration(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyConfigurationType: typeof(TestTimeoutPolicyConfiguration));
        }

        [Fact]
        public void AddTimeoutPolicyTriggersDefaultConfiguration()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 2;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldTriggerPolicyConfiguration(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyConfigurationType: typeof(DefaultTimeoutPolicyConfiguration));
        }

        [Fact]
        public void AddTimeoutPolicyTriggersCustomConfiguration()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 2;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy<TestTimeoutPolicyConfiguration>(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldTriggerPolicyConfiguration(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyConfigurationType: typeof(TestTimeoutPolicyConfiguration));
        }

        [Fact]
        public void AddTimeoutPolicyUniquePolicyPerHttpClient()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy1 = null;
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy2 = null;
            var services = new ServiceCollection();
            services
                .AddHttpClient("GitHub")
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = 1;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy1 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });
            services
                .AddHttpClient("Microsoft")
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = 2;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy2 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");
            serviceProvider.InstantiateNamedHttpClient("Microsoft");

            timeoutPolicy1.ShouldNotBeNull();
            timeoutPolicy2.ShouldNotBeNull();
            ReferenceEquals(timeoutPolicy1, timeoutPolicy2).ShouldBeFalse();
            timeoutPolicy1.PolicyKey.ShouldNotBe(timeoutPolicy2.PolicyKey);
        }
    }
}
