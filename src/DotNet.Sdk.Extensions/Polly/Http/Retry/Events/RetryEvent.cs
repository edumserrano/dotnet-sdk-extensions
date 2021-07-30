using System;
using System.Net.Http;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Events
{
    /// <summary>
    /// Contains the event data when a retry is triggered via Polly's timeout policy.
    /// </summary>
    public class RetryEvent
    {
        internal RetryEvent(
            string httpClientName,
            RetryOptions retryOptions,
            DelegateResult<HttpResponseMessage> outcome,
            TimeSpan retryDelay,
            int retryNumber,
            Context context)
        {
            HttpClientName = httpClientName;
            RetryOptions = retryOptions;
            Outcome = outcome;
            RetryDelay = retryDelay;
            RetryNumber = retryNumber;
            Context = context;
        }

        /// <summary>
        /// The name of the HttpClient that triggered this event.
        /// </summary>
        public string HttpClientName { get; }

        /// <summary>
        /// The retry options applied to the HttpClient that triggered this event.
        /// </summary>
        public RetryOptions RetryOptions { get; }

        /// <summary>
        /// Result from the HttpClient execution.
        /// </summary>
        public DelegateResult<HttpResponseMessage> Outcome { get; }

        /// <summary>
        /// The amount of time before the retry was executed.
        /// </summary>
        public TimeSpan RetryDelay { get; }

        /// <summary>
        /// The number of the retry.
        /// </summary>
        public int RetryNumber { get; }

        /// <summary>
        /// The Polly Context.
        /// </summary>
        public Context Context { get; }
    }
}
