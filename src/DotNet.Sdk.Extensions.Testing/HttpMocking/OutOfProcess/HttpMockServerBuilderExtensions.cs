namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess;

/// <summary>
/// Extension methods for the <see cref="HttpMockServerBuilder"/>.
/// </summary>
public static class HttpMockServerBuilderExtensions
{
    /// <summary>
    /// Sets the default log level for the application.
    /// </summary>
    /// <param name="httpMockServerBuilder">The <see cref="HttpMockServerBuilder"/> instance.</param>
    /// <param name="logLevel">The default log level.</param>
    /// <returns>The <see cref="HttpMockServerBuilder"/> for chaining.</returns>
    public static HttpMockServerBuilder UseDefaultLogLevel(this HttpMockServerBuilder httpMockServerBuilder, LogLevel logLevel)
    {
        if (httpMockServerBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpMockServerBuilder));
        }

        return httpMockServerBuilder.UseHostArgs("--Logging:LogLevel:Default", $"{logLevel}");
    }
}
