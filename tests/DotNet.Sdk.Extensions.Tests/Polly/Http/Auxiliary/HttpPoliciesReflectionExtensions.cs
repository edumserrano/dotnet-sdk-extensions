namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;

public static class HttpPoliciesReflectionExtensions
{
    public static IEnumerable<T> GetPolicies<T>(this HttpMessageHandlerBuilder httpMessageHandlerBuilder)
    {
        return httpMessageHandlerBuilder
            .GetPolicies()
            .OfType<T>();
    }

    public static IEnumerable<IsPolicy> GetPolicies(this HttpMessageHandlerBuilder httpMessageHandlerBuilder)
    {
        ArgumentNullException.ThrowIfNull(httpMessageHandlerBuilder);

        return httpMessageHandlerBuilder.AdditionalHandlers
            .OfType<PolicyHttpMessageHandler>()
            .Select(x => x.GetPolicy<IsPolicy>())
            .Where(x => x is not null)
            .Select(x => x!); // force the compiler to recognize that the return value is IEnumerable<IsPolicy> and not IEnumerable<IsPolicy?>. For more info see https://github.com/dotnet/roslyn/issues/37468
    }

    public static T? GetPolicy<T>(this PolicyHttpMessageHandler policyHttpMessageHandler)
    {
        return policyHttpMessageHandler.GetInstanceField<T>("_policy");
    }

    public static AsyncCircuitBreakerPolicy<HttpResponseMessage>? GetAsyncCircuitBreakerPolicy(this HttpClient httpClient)
    {
        var handler = httpClient.GetInstanceField<DelegatingHandler>(typeof(HttpMessageInvoker), "_handler");
        while (handler is not null)
        {
            if (handler is PolicyHttpMessageHandler)
            {
                var policy = handler.GetInstanceField<IAsyncPolicy<HttpResponseMessage>>("_policy");
                if (policy is AsyncPolicyWrap<HttpResponseMessage> wrapPolicy)
                {
                    var circuitBreakerPolicy = wrapPolicy.Inner as AsyncCircuitBreakerPolicy<HttpResponseMessage>;
                    if (circuitBreakerPolicy is not null)
                    {
                        return circuitBreakerPolicy;
                    }
                }
            }

            handler = handler.InnerHandler as DelegatingHandler;
        }

        return null;
    }

    public static void ResetCircuitBreakerPolicy(this HttpClient httpClient)
    {
        var circuitBreakerPolicy = httpClient.GetAsyncCircuitBreakerPolicy();
        circuitBreakerPolicy?.Reset();
    }
}
