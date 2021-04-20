using System.ComponentModel.DataAnnotations;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Timeout
{
    public class TimeoutOptions
    {
        [Range(double.Epsilon, double.MaxValue)]
        public double TimeoutInSecs { get; set; }
    }
}
