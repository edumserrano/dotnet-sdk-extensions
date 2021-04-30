using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Http;
using Polly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary.Polly
{
    public static class HttpPoliciesReflectionExtensions
    {
        public static IEnumerable<T> GetPolicies<T>(this IList<DelegatingHandler> delegatingHandlers)
        {
            return delegatingHandlers
                .GetPolicies()
                .OfType<T>();
        }
        
        public static IEnumerable<IsPolicy> GetPolicies(this IList<DelegatingHandler> delegatingHandlers)
        {
            return delegatingHandlers
                .OfType<PolicyHttpMessageHandler>()
                .Select(x => x.GetPolicy<IsPolicy>());
        }
        
        public static T GetPolicy<T>(this PolicyHttpMessageHandler policyHttpMessageHandler)
        {
            return policyHttpMessageHandler.GetInstanceField<T>("_policy");
        }
    }
}
