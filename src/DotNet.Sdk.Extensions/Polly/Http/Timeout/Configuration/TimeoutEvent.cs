using System;
using System.Threading.Tasks;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Configuration
{
    public class TimeoutEvent
    {
        internal TimeoutEvent(
            string httpClientName,
            TimeoutOptions timeoutOptions,
            Context context,
            TimeSpan requestTimeout, 
            Task timedOutTask, 
            Exception exception)
        {
            HttpClientName = httpClientName;
            TimeoutOptions = timeoutOptions;
            Context = context;
            RequestTimeout = requestTimeout;
            TimedOutTask = timedOutTask;
            Exception = exception;
        }

        public string HttpClientName { get; }

        public TimeoutOptions TimeoutOptions { get; }
        
        public Context Context { get; }
        
        public TimeSpan RequestTimeout { get; }

        public Task TimedOutTask { get; }
        
        public Exception Exception { get; }
    }
}