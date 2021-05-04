using DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;

namespace DotNet.Sdk.Extensions.Tests
{
    internal static class XUnitTestCollections
    {
        /// <summary>
        /// This is used to make sure the tests that are using <see cref="TestFallbackPolicyEventHandler"/>
        /// don't run in parallel and result in flaky tests because of the state held in the static properties.
        /// </summary>
        public const string FallbackPolicy = "FallbackPolicy";

        /// <summary>
        /// This is used to make sure the tests that are using <see cref="TestRetryPolicyEventHandler"/>
        /// don't run in parallel and result in flaky tests because of the state held in the static properties.
        /// </summary>
        public const string RetryPolicy = "RetryPolicy";

        /// <summary>
        /// This is used to make sure the tests that are using <see cref="TestCircuitBreakerPolicyEventHandler"/>
        /// don't run in parallel and result in flaky tests because of the state held in the static properties.
        /// </summary>
        public const string CircuitBreakerPolicy = "CircuitBreakerPolicy";
        
        /// <summary>
        /// This is used to make sure the tests that are using <see cref="TestTimeoutPolicyEventHandler"/>
        /// don't run in parallel and result in flaky tests because of the state held in the static properties.
        /// </summary>
        public const string TimeoutPolicy = "TimeoutPolicy";
    }
}
