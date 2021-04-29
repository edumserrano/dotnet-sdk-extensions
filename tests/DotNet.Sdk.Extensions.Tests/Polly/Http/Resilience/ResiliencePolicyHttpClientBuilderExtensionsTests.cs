using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.Retry;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience
{
    public class ResiliencePolicyHttpClientBuilderExtensionsTests
    {
        [Fact]
        public void ExampleReflectionTest()
        {
            var policyKey = "testPolicy";
            var optionsName = "circuitBreakerOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(name: "GitHubResilienceOptions")
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 1;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 4;
                });
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientResiliencePolicies(
                    policyKey: "GitHub",
                    optionsName: "GitHubResilienceOptions",
                    provider);
            });

            AsyncRetryPolicy<HttpResponseMessage>? asyncRetryPolicy = null;
            services
                .AddHttpClient("GitHub")
                .AddResiliencePoliciesFromRegistry(policyKey: "GitHub")
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    asyncRetryPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");
            asyncRetryPolicy.ShouldNotBeNull();

            asyncRetryPolicy.GetRetryCount().ShouldBe(3);
            asyncRetryPolicy.GetMedianFirstRetryDelay().ShouldBe(TimeSpan.FromSeconds(1));

            var exceptionPredicates = asyncRetryPolicy.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(3);
            exceptionPredicates.HandlesException<TaskCanceledException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<TimeoutRejectedException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<HttpRequestException>().ShouldBeTrue();

            var resultPredicates = asyncRetryPolicy.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(1);
            resultPredicates.HandlesTransientHttpStatusCode().ShouldBe(true);
        }

    }
}
