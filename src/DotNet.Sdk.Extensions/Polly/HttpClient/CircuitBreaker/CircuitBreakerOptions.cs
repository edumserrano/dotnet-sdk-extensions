using System.ComponentModel.DataAnnotations;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker
{
    public class CircuitBreakerOptions
    {
        [Range(double.Epsilon, 1)]
        public double FailureThreshold { get; set; }

        [Range(double.Epsilon, double.MaxValue)]
        public double SamplingDurationInSecs { get; set; }

        [Range(2, int.MaxValue)]
        public int MinimumThroughput { get; set; }

        [Range(double.Epsilon, double.MaxValue)]
        public double DurationOfBreakInSecs { get; set; }
    }
}