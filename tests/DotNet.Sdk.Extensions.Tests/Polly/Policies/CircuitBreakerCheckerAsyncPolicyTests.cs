using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Policies;
using NSubstitute;
using Polly;
using Polly.CircuitBreaker;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Policies
{
    [Trait("Category", XUnitCategories.Polly)]
    public class CircuitBreakerCheckerAsyncPolicyTests
    {
        [Fact]
        public void ValidateArguments()
        {
            var exception1 = Should.Throw<ArgumentNullException>(() =>
            {
                _ = CircuitBreakerCheckerAsyncPolicy.Create(
                    circuitBreakerPolicy: null!,
                    fallbackValueFactory: (circuitBreakerState, context, token) => Task.FromResult(1));
            });
            exception1.Message.ShouldBe("Value cannot be null. (Parameter 'circuitBreakerPolicy')");

            var exception2 = Should.Throw<ArgumentNullException>(() =>
            {
                _ = CircuitBreakerCheckerAsyncPolicy.Create<int>(
                    circuitBreakerPolicy: Substitute.For<ICircuitBreakerPolicy>(),
                    fallbackValueFactory: null!);
            });
            exception2.Message.ShouldBe("Value cannot be null. (Parameter 'fallbackValueFactory')");
        }

        [Fact]
        public async Task CircuitBreakerCheckerDoesNothingIfCircuitIsClosed()
        {
            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: 2, durationOfBreak: TimeSpan.FromMinutes(1));
            var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy.Create(
                circuitBreakerPolicy: circuitBreakerPolicy,
                fallbackValueFactory: (circuitBreakerState, context, token) => Task.FromResult(1));

            // when the circuit breaker of the circuit breaker policy is not open
            // the circuit breaker checker policy will not do anything
            var policyResult1 = await circuitBreakerCheckerPolicy.ExecuteAsync(() => Task.FromResult(2));
            policyResult1.ShouldBe(2);
        }

        [Fact]
        public async Task CircuitBreakerCheckerWhenCircuitIsIsolated()
        {
            CircuitBreakerState? circuitBreakerState = null;
            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: 2, durationOfBreak: TimeSpan.FromMinutes(1));
            var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy.Create(
                circuitBreakerPolicy: circuitBreakerPolicy,
                fallbackValueFactory: (state, context, token) =>
                {
                    circuitBreakerState = state;
                    return Task.FromResult(1);
                });

            // when the circuit breaker of the circuit breaker policy is open
            // the circuit breaker checker policy will not call the action
            // that is being wrapped by the circuit breaker but will return
            // the value produced by the factory of the circuit breaker checker policy.
            // note that without the checker what should happen is that the circuit breaker
            // policy would throw an exception if the circuit is open/isolated.
            circuitBreakerPolicy.Isolate();
            _ = await Should.ThrowAsync<IsolatedCircuitException>(() =>
              {
                  return circuitBreakerPolicy.ExecuteAsync(() => Task.FromResult(2));
              });
            var policyResult = await circuitBreakerCheckerPolicy.ExecuteAsync(() => Task.FromResult(2));
            policyResult.ShouldBe(1);
            circuitBreakerState.ShouldBe(CircuitBreakerState.Isolated);
        }

        [Fact]
        public async Task CircuitBreakerCheckerWhenCircuitIsOpen()
        {
            CircuitBreakerState? circuitBreakerState = null;
            var exceptionsAllowedBeforeBreaking = 2;
            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: exceptionsAllowedBeforeBreaking, durationOfBreak: TimeSpan.FromMinutes(1));
            var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy.Create(
                circuitBreakerPolicy: circuitBreakerPolicy,
                fallbackValueFactory: (state, context, token) =>
                {
                    circuitBreakerState = state;
                    return Task.FromResult(1);
                });

            for (var i = 0; i < exceptionsAllowedBeforeBreaking; i++)
            {
                _ = await circuitBreakerPolicy.ExecuteAndCaptureAsync(() => throw new Exception("test"));
            }
            _ = await Should.ThrowAsync<BrokenCircuitException>(() =>
              {
                  return circuitBreakerPolicy.ExecuteAsync(() => Task.FromResult(2));
              });
            var policyResult = await circuitBreakerCheckerPolicy.ExecuteAsync(() => Task.FromResult(2));
            policyResult.ShouldBe(1);
            circuitBreakerState.ShouldBe(CircuitBreakerState.Open);
        }
    }
}
