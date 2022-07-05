using Microsoft.AspNetCore.Hosting;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary;

internal static class WebHostBuilderExtensions
{
    // This is used by tests to make sure a random port is selected or tests might fail
    // when running concurrently.
    // Without this, tests can fail start a host because the port is already in use.
    public static IWebHostBuilder UseLocalhostWithRandomPort(this IWebHostBuilder webHostBuilder)
    {
        return webHostBuilder.UseUrls("http://*:0;https://*:0");
    }
}
