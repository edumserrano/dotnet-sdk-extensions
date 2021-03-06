# AmbientDataDemo project readme

This project contains a demo app for the [Share data across the lifetime of an HTTP request](/docs/guides/share-data-with-async-local.md.md) guide.

This app is an ASP.NET web api that shows how to use `AsyncLocal<T>` to share ambient data accross the lifetime of an HTTP Request.

## How to run

* Open the `DotNet.Sdk.Extensions.Demos.sln`.
* From Visual Studio, set the `demos/guides/AmbientDataDemo/AmbientDataDemo.cspproj` project as the Startup Project.
* Run the project.
* The browser should open on the swagger page at https://localhost:5001/swagger/index.html.
* Execute the `/api/ambient/demo` endpoint by clicking on it, then on the *Try it Out* button and finally on the *Execute* button.
