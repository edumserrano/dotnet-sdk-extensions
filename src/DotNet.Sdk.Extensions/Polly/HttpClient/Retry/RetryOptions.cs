namespace DotNet.Sdk.Extensions.Polly.HttpClient.Retry
{
    public class RetryOptions
    {
        public int RetryCount { get; set; }

        public double MedianFirstRetryDelayInSecs { get; set; }
    }
}
