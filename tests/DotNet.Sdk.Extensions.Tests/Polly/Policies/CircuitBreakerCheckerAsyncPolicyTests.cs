using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Policies;
using NSubstitute;
using Polly;
using Polly.CircuitBreaker;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Policies
{
    [Trait("Category", "Polly")]
    public class CircuitBreakerCheckerAsyncPolicyTests
    {
        [Fact]
        public void ValidateArguments()
        {
            var exception1 = Should.Throw<ArgumentNullException>(() =>
            {
                CircuitBreakerCheckerAsyncPolicy<int>.Create(
                    circuitBreakerPolicy: null!,
                    factory: (context, token) => Task.FromResult(1));
            });
            exception1.Message.ShouldBe("Value cannot be null. (Parameter 'circuitBreakerPolicy')");
            
            var exception2 = Should.Throw<ArgumentNullException>(() =>
            {
                CircuitBreakerCheckerAsyncPolicy<int>.Create(
                    circuitBreakerPolicy: Substitute.For<ICircuitBreakerPolicy>(),
                    factory: null!);
            });
            exception2.Message.ShouldBe("Value cannot be null. (Parameter 'factory')");
        }

        [Fact]
        public async Task CircuitBreakerCheckerDoesNothingIfCircuitIsClosed()
        {
            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: 2, durationOfBreak: TimeSpan.FromMinutes(1));
            var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy<int>.Create(
                circuitBreakerPolicy: circuitBreakerPolicy,
                factory: (context, token) => Task.FromResult(1));

            // when the circuit breaker of the circuit breaker policy is not open
            // the circuit breaker checker policy will not do anything
            var policyResult1 = await circuitBreakerCheckerPolicy.ExecuteAsync(() => Task.FromResult(2));
            policyResult1.ShouldBe(2);
        }

        [Fact]
        public async Task CircuitBreakerCheckerWhenCircuitIsIsolated()
        {
            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: 2, durationOfBreak: TimeSpan.FromMinutes(1));
            var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy<int>.Create(
                circuitBreakerPolicy: circuitBreakerPolicy,
                factory: (context, token) => Task.FromResult(1));

            // when the circuit breaker of the circuit breaker policy is open
            // the circuit breaker checker policy will not call the action
            // that is being wrapped by the circuit breaker but will return
            // the value produced by the factory of the circuit breaker checker policy.
            // note that without the checker what should happen is that the circuit breaker
            // policy would throw an exception if the circuit is open/isolated.
            circuitBreakerPolicy.Isolate();
            await Should.ThrowAsync<IsolatedCircuitException>(() =>
            {
                return circuitBreakerPolicy.ExecuteAsync(() => Task.FromResult(2));
            });
            var policyResult = await circuitBreakerCheckerPolicy.ExecuteAsync(() => Task.FromResult(2));
            policyResult.ShouldBe(1);
        }

        [Fact]
        public async Task CircuitBreakerCheckerWhenCircuitIsOpen()
        {
            var exceptionsAllowedBeforeBreaking = 2;
            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: exceptionsAllowedBeforeBreaking, durationOfBreak: TimeSpan.FromMinutes(1));
            var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy<int>.Create(
                circuitBreakerPolicy: circuitBreakerPolicy,
                factory: (context, token) => Task.FromResult(1));

            for (var i = 0; i < exceptionsAllowedBeforeBreaking; i++)
            {
                await circuitBreakerPolicy.ExecuteAndCaptureAsync(() => throw new Exception("test"));
            }
            await Should.ThrowAsync<BrokenCircuitException>(() =>
            {
                return circuitBreakerPolicy.ExecuteAsync(() => Task.FromResult(2));
            });
            var policyResult = await circuitBreakerCheckerPolicy.ExecuteAsync(() => Task.FromResult(2));
            policyResult.ShouldBe(1);
        }
    }
}
