using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary
{
    public static class HttpPoliciesServiceProviderExtensions
    {
        public static T GetHttpPolicy<T>(this ServiceProvider serviceProvider, string policyKey) where T : IsPolicy
        {
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var timeoutPolicy = registry.Get<T>(policyKey);
            return timeoutPolicy;
        }

        public static HttpClient InstantiateNamedHttpClient(this ServiceProvider serviceProvider, string name)
        {
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            return httpClientFactory.CreateClient(name);
        }
    }
}
