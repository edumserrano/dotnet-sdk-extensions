using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly;
using Polly.Fallback;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary
{
    public static class AsyncFallbackPolicyReflectionExtensions
    {
        public static OnFallbackTarget GetOnFallbackTarget<T>(this AsyncFallbackPolicy<T> policy)
        {
            return new OnFallbackTarget(policy);
        }

        public class OnFallbackTarget
        {
            public OnFallbackTarget(IsPolicy policy)
            {
                var onTimeoutAsync = policy
                    .GetInstanceField("_onFallbackAsync")
                    .GetInstanceProperty("Target");
                HttpClientName = onTimeoutAsync.GetInstanceField<string>("httpClientName");
                PolicyEventHandler = onTimeoutAsync.GetInstanceField<IFallbackPolicyEventHandler>("policyEventHandler");
            }

            public string HttpClientName { get; }
            
            public IFallbackPolicyEventHandler PolicyEventHandler { get; }
        }
    }
}