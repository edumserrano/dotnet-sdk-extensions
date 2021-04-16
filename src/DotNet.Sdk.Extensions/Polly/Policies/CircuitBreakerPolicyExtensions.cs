using System;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.Policies
{
    internal static class CircuitBreakerPolicyExtensions
    {
        public static bool IsCircuitOpen(this ICircuitBreakerPolicy circuitBreakerPolicy)
        {
            if (circuitBreakerPolicy == null) throw new ArgumentNullException(nameof(circuitBreakerPolicy));

            return circuitBreakerPolicy.CircuitState switch
            {
                CircuitState.Open => true,
                CircuitState.Isolated => true,
                _ => false
            };
        }
    }
}
