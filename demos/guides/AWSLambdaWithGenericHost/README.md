# AWSLambdaWithGenericHost project readme

This project contains a demo app for the [Use cases for generic host](/docs/guides/generic-host-use-cases.md) guide.

This app is an AWS Lambda created with the template for an empty function. The template was modified to make use of the generic host to provide dependency injection, logging and configuration via appsettings.

*Note:* for the configuration values to be loaded from the appsettings.json file, it's properties must be configured to have the `Build Action` set to `Content` and the `Copy to Output Directory` to `Copy if newer`.

## How to run

* Open the `DotNet.Sdk.Extensions.Demos.sln`.
* From Visual Studio, set the `demos/guides/AWSLambdaWithGenericHost/AWSLambdaWithGenericHost.cspproj` project as the Startup Project.
* Run the project.
