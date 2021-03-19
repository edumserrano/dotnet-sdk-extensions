namespace DotNet.Sdk.Extensions.Polly.HttpClient.Options
{
    public class RetryOptions
    {
        public int RetryCount { get; set; }

        public double MedianFirstRetryDelayInSecs { get; set; }
    }
}
