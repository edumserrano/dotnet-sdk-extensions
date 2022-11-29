namespace DotNet.Sdk.Extensions.Polly.Http.Resilience;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Ignore for IValidateOptions implementations. Used as generic type param.")]
internal sealed class ResilienceOptionsValidation : IValidateOptions<ResilienceOptions>
{
    public ValidateOptionsResult Validate(string name, ResilienceOptions options)
    {
        Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);

        if (options.EnableRetryPolicy)
        {
            Validator.ValidateObject(options.Retry, new ValidationContext(options.Retry), validateAllProperties: true);
        }

        if (options.EnableCircuitBreakerPolicy)
        {
            Validator.ValidateObject(options.CircuitBreaker, new ValidationContext(options.CircuitBreaker), validateAllProperties: true);
        }

        if (options.EnableTimeoutPolicy)
        {
            Validator.ValidateObject(options.Timeout, new ValidationContext(options.Timeout), validateAllProperties: true);
        }

        return ValidateOptionsResult.Success;
    }
}
