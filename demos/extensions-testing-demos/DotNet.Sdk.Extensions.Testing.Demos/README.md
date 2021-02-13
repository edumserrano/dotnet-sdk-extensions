# DotNet.Sdk.Extensions.Testing.Demos project readme

This project contains tests to demo the usage of the testing extensions for [integration tests](/README.md#for-integration-tests) and [unit tests](/README.md#for-unit-tests) provided by this repo.

Because this test project contains multiple `Startup` classes we have to implement a custom `WebApplicationFactory` class to make sure we cam use the intended `Startup` class for each scenario. For more information read [Notes on WebApplicationFactory regarding asp.net integration tests](/docs/integration-tests/web-application-factory.md).