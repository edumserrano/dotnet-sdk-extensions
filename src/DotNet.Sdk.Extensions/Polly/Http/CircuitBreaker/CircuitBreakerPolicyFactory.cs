namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;

internal static class CircuitBreakerPolicyFactory
{
    public static AsyncPolicyWrap<HttpResponseMessage> CreateCircuitBreakerPolicy(
        string httpClientName,
        CircuitBreakerOptions options,
        ICircuitBreakerPolicyEventHandler policyEventHandler)
    {
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .Or<TaskCanceledException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: options.FailureThreshold,
                samplingDuration: TimeSpan.FromSeconds(options.SamplingDurationInSecs),
                minimumThroughput: options.MinimumThroughput,
                durationOfBreak: TimeSpan.FromSeconds(options.DurationOfBreakInSecs),
#pragma warning disable MA0147 // Avoid async void method for delegate
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
                onBreak: async (lastOutcome, previousState, breakDuration, context) =>
                {
                    var breakEvent = new BreakEvent(
                        httpClientName,
                        options,
                        lastOutcome,
                        previousState,
                        breakDuration,
                        context);
                    await policyEventHandler.OnBreakAsync(breakEvent);
                },
                onReset: async context =>
                {
                    var resetEvent = new ResetEvent(httpClientName, options, context);
                    await policyEventHandler.OnResetAsync(resetEvent);
                },
                onHalfOpen: async () =>
                {
                    var halfOpenEvent = new HalfOpenEvent(httpClientName, options);
                    await policyEventHandler.OnHalfOpenAsync(halfOpenEvent);
                });
#pragma warning restore MA0147 // Avoid async void method for delegate
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
        var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy.Create(
            circuitBreakerPolicy: circuitBreakerPolicy,
            fallbackValueFactory: (circuitBreakerState, _, _) =>
            {
                return Task.FromResult<HttpResponseMessage>(new CircuitBrokenHttpResponseMessage(circuitBreakerState));
            });
        return Policy.WrapAsync(circuitBreakerCheckerPolicy, circuitBreakerPolicy);
    }
}
