using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience
{
    internal class ResilienceOptionsValidation : IValidateOptions<ResilienceOptions>
    {
        public ValidateOptionsResult Validate(string name, ResilienceOptions options)
        {
            Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);
            Validator.ValidateObject(options.Timeout, new ValidationContext(options.Timeout), validateAllProperties: true);
            Validator.ValidateObject(options.Retry, new ValidationContext(options.Retry), validateAllProperties: true);
            Validator.ValidateObject(options.CircuitBreaker, new ValidationContext(options.CircuitBreaker), validateAllProperties: true);
            return ValidateOptionsResult.Success;
        }
    }
}