using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Polly.Wrap;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback
{
    internal static class FallbackPolicyFactory
    {
        public static AsyncPolicyWrap<HttpResponseMessage> CreateFallbackPolicy(
            string httpClientName,
            IFallbackPolicyConfiguration policyConfiguration)
        {
            // handle TimeoutRejectedException thrown by a timeout policy
            var timeoutFallback = Policy<HttpResponseMessage>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(
                    fallbackAction: (delegateResult, pollyContext, cancellationToken) =>
                    {
                        var exception = (TimeoutRejectedException)delegateResult.Exception;
                        return Task.FromResult<HttpResponseMessage>(new TimeoutHttpResponseMessage(exception));
                    },
                    onFallbackAsync: (outcome, context) =>
                    {
                        var fallbackEvent = new TimeoutFallbackEvent(httpClientName, outcome, context);
                        return policyConfiguration.OnTimeoutFallbackAsync(fallbackEvent);
                    });

            // handle BrokenCircuitException thrown by a circuit breaker policy
            var brokenCircuitFallback = Policy<HttpResponseMessage>
                .Handle<BrokenCircuitException>()
                .Or<IsolatedCircuitException>()
                .FallbackAsync(
                    fallbackAction: (delegateResult, pollyContext, cancellationToken) =>
                    {
                        var circuitBrokenHttpResponseMessage = new CircuitBrokenHttpResponseMessage();
                        return Task.FromResult<HttpResponseMessage>(circuitBrokenHttpResponseMessage);
                    }, 
                    onFallbackAsync: (outcome, context) =>
                    {
                        var fallbackEvent = new BrokenCircuitFallbackEvent(httpClientName, outcome, context);
                        return policyConfiguration.OnBrokenCircuitFallbackAsync(fallbackEvent);
                    });

            // handle TaskCanceledException thrown by HttpClient when it times out.
            // on newer versions .NET still throws TaskCanceledException but the inner exception is of type System.TimeoutException.
            // see https://devblogs.microsoft.com/dotnet/net-5-new-networking-improvements/#better-error-handling
            var abortedFallback = Policy<HttpResponseMessage>
                .Handle<TaskCanceledException>()
                .FallbackAsync(
                    fallbackAction: (delegateResult, pollyContext, cancellationToken) =>
                    {
                        var exception = (TaskCanceledException)delegateResult.Exception;
                        return Task.FromResult<HttpResponseMessage>(new AbortedHttpResponseMessage(exception));
                    },
                    onFallbackAsync: (outcome, context) =>
                    {
                        var fallbackEvent = new TaskCancelledFallbackEvent(httpClientName, outcome, context);
                        return policyConfiguration.OnTaskCancelledFallbackAsync(fallbackEvent);
                    });

            var policy = Policy.WrapAsync(timeoutFallback, brokenCircuitFallback, abortedFallback);
            return policy;
        }
    }
}
