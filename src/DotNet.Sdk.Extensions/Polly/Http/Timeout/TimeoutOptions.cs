using System.ComponentModel.DataAnnotations;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout
{
    public class TimeoutOptions
    {
        [Range(double.Epsilon, double.MaxValue)]
        public double TimeoutInSecs { get; set; }
    }
}
