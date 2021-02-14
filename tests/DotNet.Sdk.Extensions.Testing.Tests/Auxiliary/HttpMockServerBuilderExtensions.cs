using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess;
using Microsoft.Extensions.Logging;

namespace DotNet.Sdk.Extensions.Testing.Tests.Auxiliary
{
    internal static class HttpMockServerBuilderExtensions
    {
        // used to avoid logs from the server showing up on test output
        public static HttpMockServerBuilder SetDefaultLogLevel(this HttpMockServerBuilder httpMockServerBuilder, LogLevel logLevel)
        {
            return httpMockServerBuilder.UseHostArgs("--Logging:LogLevel:Default", $"{logLevel}");
        }
    }
}