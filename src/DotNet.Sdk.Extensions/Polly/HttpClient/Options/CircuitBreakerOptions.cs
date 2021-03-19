namespace DotNet.Sdk.Extensions.Polly.HttpClient.Options
{
    public class CircuitBreakerOptions
    {
        public double FailureThreshold { get; set; }

        public double SamplingDurationInSecs { get; set; }

        public int MinimumThroughput { get; set; }

        public double DurationOfBreakInSecs { get; set; }
    }
}