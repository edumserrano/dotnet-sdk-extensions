using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.FallbackHttpResponseMessages;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.Extensions
{
    public static class FallbackPolicyRegistryExtensions
    {
        public static IPolicyRegistry<string> AddHttpClientFallbackPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            IServiceProvider serviceProvider)
        {
            return registry.AddHttpClientFallbackPolicy<DefaultFallbackPolicyConfiguration>(policyKey, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientFallbackPolicy<TPolicyConfiguration>(
            this IPolicyRegistry<string> registry,
            string policyKey,
            IServiceProvider serviceProvider) where TPolicyConfiguration : class, IFallbackPolicyConfiguration
        {
            // by choice, TPolicyConfiguration is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<TPolicyConfiguration>(serviceProvider);

            // handle TimeoutRejectedException thrown by a timeout policy
            var timeoutFallback = Policy<HttpResponseMessage>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(
                    fallbackValue: new TimeoutHttpResponseMessage(),
                    onFallbackAsync: (outcome, context) =>
                    {
                        return policyConfiguration.OnTimeoutFallbackAsync(outcome, context);
                    });

            // handle BrokenCircuitException thrown by a circuit breaker policy
            var brokenCircuitFallback = Policy<HttpResponseMessage>
                .Handle<BrokenCircuitException>()
                .FallbackAsync(
                    fallbackValue: new CircuitBrokenHttpResponseMessage(),
                    onFallbackAsync: (outcome, context) =>
                    {
                        return policyConfiguration.OnBrokenCircuitFallbackAsync(outcome, context);
                    });

            // handle TaskCanceledException thrown by HttpClient when it times out.
            // on newer versions .NET still throws TaskCanceledException but the inner exception is of type System.TimeoutException.
            // see https://devblogs.microsoft.com/dotnet/net-5-new-networking-improvements/#better-error-handling
            var abortedFallback = Policy<HttpResponseMessage>
                .Handle<TaskCanceledException>()
                .FallbackAsync(
                    fallbackAction: (delegateResult, pollyContext, cancellationToken) =>
                    {
                        var innerException = delegateResult.Exception?.InnerException;
                        var triggeredByTimeoutException = innerException is TimeoutException;
                        return Task.FromResult<HttpResponseMessage>(new AbortedHttpResponseMessage(triggeredByTimeoutException));
                    },
                    onFallbackAsync: (outcome, context) =>
                    {
                        return policyConfiguration.OnTaskCancelledFallbackAsync(outcome, context);
                    });

            var policy = Policy.WrapAsync(timeoutFallback, brokenCircuitFallback, abortedFallback);
            registry.Add(key: policyKey, policy);
            return registry;
        }
    }
}

