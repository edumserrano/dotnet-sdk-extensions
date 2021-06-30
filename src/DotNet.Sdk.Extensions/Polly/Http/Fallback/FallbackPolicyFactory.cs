using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
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
            IFallbackPolicyEventHandler policyEventHandler)
        {
            // handle HttpRequestException thrown by the HttpClient
            var httpRequestExceptionFallback = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .FallbackAsync(
                    fallbackAction: (delegateResult, pollyContext, cancellationToken) =>
                    {
                        var response = new ExceptionHttpResponseMessage(delegateResult.Exception);
                        return Task.FromResult<HttpResponseMessage>(response);
                    },
                    onFallbackAsync: (outcome, context) =>
                    {
                        var fallbackEvent = new FallbackEvent(httpClientName, outcome, context);
                        return policyEventHandler.OnHttpRequestExceptionFallbackAsync(fallbackEvent);
                    });

            // handle TimeoutRejectedException thrown by a timeout policy
            var timeoutFallback = Policy<HttpResponseMessage>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(
                    fallbackAction: (delegateResult, pollyContext, cancellationToken) =>
                    {
                        var response = new TimeoutHttpResponseMessage(delegateResult.Exception);
                        return Task.FromResult<HttpResponseMessage>(response);
                    },
                    onFallbackAsync: (outcome, context) =>
                    {
                        var fallbackEvent = new FallbackEvent(httpClientName, outcome, context);
                        return policyEventHandler.OnTimeoutFallbackAsync(fallbackEvent);
                    });

            // handle BrokenCircuitException thrown by a circuit breaker policy
            var brokenCircuitFallback = Policy<HttpResponseMessage>
                .Handle<BrokenCircuitException>()
                .Or<IsolatedCircuitException>()
                .FallbackAsync(
                    fallbackAction: (delegateResult, pollyContext, cancellationToken) =>
                    {
                        var exception = delegateResult.Exception;
                        var response = exception switch
                        {
                            IsolatedCircuitException => new CircuitBrokenHttpResponseMessage(CircuitBreakerState.Isolated, exception),
                            BrokenCircuitException => new CircuitBrokenHttpResponseMessage(CircuitBreakerState.Open, exception),
                            _ => throw new ArgumentOutOfRangeException(nameof(delegateResult), $"FallbackPolicyFactory: unexpected exception of type {delegateResult.Exception.GetType()}")
                        };
                        return Task.FromResult<HttpResponseMessage>(response);
                    },
                    onFallbackAsync: (outcome, context) =>
                    {
                        var fallbackEvent = new FallbackEvent(httpClientName, outcome, context);
                        return policyEventHandler.OnBrokenCircuitFallbackAsync(fallbackEvent);
                    });

            // handle TaskCanceledException thrown by HttpClient when it times out.
            var abortedFallback = Policy<HttpResponseMessage>
                .Handle<TaskCanceledException>()
                .FallbackAsync(
                    fallbackAction: (delegateResult, pollyContext, cancellationToken) =>
                    {
                        // on newer versions .NET still throws TaskCanceledException but the inner exception is of type System.TimeoutException.
                        // see https://devblogs.microsoft.com/dotnet/net-5-new-networking-improvements/#better-error-handling
                        var exception = delegateResult.Exception;
                        return exception switch
                        {
                            { InnerException: TimeoutException } => Task.FromResult<HttpResponseMessage>(new TimeoutHttpResponseMessage(exception)),
                            _ => Task.FromResult<HttpResponseMessage>(new AbortedHttpResponseMessage(exception))
                        };
                    },
                    onFallbackAsync: (outcome, context) =>
                    {
                        var fallbackEvent = new FallbackEvent(httpClientName, outcome, context);
                        return policyEventHandler.OnTaskCancelledFallbackAsync(fallbackEvent);
                    });

            var policy = Policy.WrapAsync(httpRequestExceptionFallback,
                timeoutFallback,
                brokenCircuitFallback,
                abortedFallback);
            return policy;
        }
    }
}
