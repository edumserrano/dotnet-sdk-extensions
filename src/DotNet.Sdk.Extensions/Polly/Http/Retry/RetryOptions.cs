using System.ComponentModel.DataAnnotations;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry
{
    public class RetryOptions
    {
        [Range(0, int.MaxValue)]
        public int RetryCount { get; set; }

        [Range(double.Epsilon, double.MaxValue)]
        public double MedianFirstRetryDelayInSecs { get; set; }
    }
}
