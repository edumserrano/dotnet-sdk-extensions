using System.Net.Http;
using System.Threading.Tasks;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback
{
    public interface IFallbackPolicyConfiguration
    {
        Task OnTimeoutFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context);
        
        Task OnBrokenCircuitFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context);
        
        Task OnTaskCancelledFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context);
    }
}