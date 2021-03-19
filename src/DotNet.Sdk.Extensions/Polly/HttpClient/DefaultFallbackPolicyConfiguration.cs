using System.Net.Http;
using System.Threading.Tasks;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    internal class DefaultFallbackPolicyConfiguration : IFallbackPolicyConfiguration
    {
        public Task OnTimeoutFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context)
        {
            return Task.CompletedTask;
        }

        public Task OnBrokenCircuitFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context)
        {
            return Task.CompletedTask;
        }

        public Task OnTaskCancelledFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context)
        {
            return Task.CompletedTask;
        }
    }
}