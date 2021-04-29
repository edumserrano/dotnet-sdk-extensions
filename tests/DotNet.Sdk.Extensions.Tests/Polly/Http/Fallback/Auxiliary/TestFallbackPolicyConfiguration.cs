using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback;
using Polly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary
{
    public class TestFallbackPolicyConfiguration : IFallbackPolicyConfiguration
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
