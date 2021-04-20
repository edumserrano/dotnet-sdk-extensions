namespace DotNet.Sdk.Extensions.Polly.HttpClient.Resilience.Extensions
{
    internal static class ResiliencePolicyKeys
    {
        public static string Timeout(string policyKey) => $"{policyKey}-timeout";
        public static string Retry(string policyKey) => $"{policyKey}-retry";
        public static string CircuitBreaker(string policyKey) => $"{policyKey}-circuit-breaker";
        public static string Fallback(string policyKey) => $"{policyKey}-fallback";
    }
}