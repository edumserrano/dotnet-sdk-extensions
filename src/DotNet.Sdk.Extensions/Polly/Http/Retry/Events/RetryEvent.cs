using System;
using System.Net.Http;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Events
{
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

        public string HttpClientName { get; }

        public RetryOptions RetryOptions { get; }
        
        public DelegateResult<HttpResponseMessage> Outcome { get; }
        
        public TimeSpan RetryDelay { get; }
        
        public int RetryNumber { get; }
        
        public Context Context { get; }
    }
}