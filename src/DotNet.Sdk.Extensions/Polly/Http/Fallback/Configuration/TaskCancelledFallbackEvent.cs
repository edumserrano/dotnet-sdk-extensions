using System.Net.Http;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Configuration
{
    public class TaskCancelledFallbackEvent
    {
        internal TaskCancelledFallbackEvent(
            string httpClientName,
            DelegateResult<HttpResponseMessage> outcome,
            Context context)
        {
            HttpClientName = httpClientName;
            Outcome = outcome;
            Context = context;
        }

        public string HttpClientName { get; }

        public DelegateResult<HttpResponseMessage> Outcome { get; }
        
        public Context Context { get; }
    }
}