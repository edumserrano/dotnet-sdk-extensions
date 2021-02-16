# dot-net-sdk-extensions docs

## Extensions list

The extensions available are split into two groups:

* Extensions to use on app code.
* Extensions to use when doing integration and unit tests.

For more information about each extension check its docs and demo. You can find the link to each extension's documentation below.

### For apps

* [Eagerly validating options](/docs/configuration/options-eagerly-validation.md)
* [Using `T` options classes instead of `IOptions<T>`](/docs/configuration/options-without-IOptions.md)

### For integration tests

* [Providing test appsettings files to the test server](/docs/integration-tests/configuring-webhost.md)
* [Mocking HttpClient's responses in-process](/docs/integration-tests/http-mocking-in-process.md)
* [Mocking HttpClient's responses out-of-process](/docs/integration-tests/http-mocking-out-of-process.md)
* [Integration tests for HostedServices (Background Services)](/docs/integration-tests/hosted-services.md)

### For unit tests

* [Mocking HttpClient's responses for unit testing](/docs/unit-tests/http-mocking-unit-tests.md)

### Other

* [Notes on WebApplicationFactory regarding asp.net integration tests](/docs/integration-tests/web-application-factory.md)
