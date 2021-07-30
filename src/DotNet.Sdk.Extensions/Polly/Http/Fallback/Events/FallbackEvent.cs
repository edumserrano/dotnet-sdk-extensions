using System.Net.Http;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Events
{
    /// <summary>
    /// Contains the event data when a fallback <see cref="HttpResponseMessage"/> is returned via Polly's fallback policy.
    /// </summary>
    public class FallbackEvent
    {
        internal FallbackEvent(
            string httpClientName,
            DelegateResult<HttpResponseMessage> outcome,
            Context context)
        {
            HttpClientName = httpClientName;
            Outcome = outcome;
            Context = context;
        }

        /// <summary>
        /// The name of the HttpClient that triggered this event.
        /// </summary>
        public string HttpClientName { get; }

        /// <summary>
        /// Result from the HttpClient execution.
        /// </summary>
        public DelegateResult<HttpResponseMessage> Outcome { get; }

        /// <summary>
        /// The Polly Context.
        /// </summary>
        public Context Context { get; }
    }
}
