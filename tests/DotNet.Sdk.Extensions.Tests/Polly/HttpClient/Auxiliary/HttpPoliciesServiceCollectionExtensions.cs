using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;

namespace DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Auxiliary
{
    public static class HttpPoliciesServiceCollectionExtensions
    {
        public static T GetHttpPolicy<T>(this ServiceCollection services, string policyKey) where T : IsPolicy
        {
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var timeoutPolicy = registry.Get<T>(policyKey);
            return timeoutPolicy;
        }
    }
}
