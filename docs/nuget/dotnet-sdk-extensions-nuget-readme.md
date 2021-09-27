# DotNet-Sdk-Extensions

## Description

This package provides extensions to help build .NET applications, using .net core 3.1 and higher.

The extensions provided by this package are:

* [Eagerly validating options](../configuration/options-eagerly-validation.md)
* [Using `T` options classes instead of `IOptions<T>`](../configuration/options-without-IOptions.md)
* Extending [Polly](https://github.com/App-vNext/Polly)
  * [Circuit breaker checker policy](../polly/circuit-breaker-checker-policy.md)
  * [Add a timeout policy to an HttpClient](../polly/httpclient-with-timeout-policy.md)
  * [Add a retry policy to an HttpClient](../polly/httpclient-with-retry-policy.md)
  * [Add a circuit breaker policy to an HttpClient](../polly/httpclient-with-circuit-breaker-policy.md)
  * [Add a fallback policy to an HttpClient](../polly/httpclient-with-fallback-policy.md)
  * [Add a set of resilience policies to an HttpClient](../polly/httpclient-with-resilience-policies.md)
  * [Extending the policy options validation](../polly/extending-policy-options-validation.md)

For more information on how to get started see the docs provided for each extension.

## Feedback

If you'd like to contribute or leave feedback please open an issue on the [package's repo](https://github.com/edumserrano/dot-net-sdk-extensions).
