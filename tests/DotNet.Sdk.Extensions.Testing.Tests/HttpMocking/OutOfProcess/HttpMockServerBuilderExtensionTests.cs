namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.OutOfProcess;

[Trait("Category", XUnitCategories.HttpMockingOutOfProcess)]
public class HttpMockServerBuilderExtensionTests
{
    /// <summary>
    /// Tests that there is no default value set for the configuration key "Logging:LogLevel:Default".
    /// This is important because it serves as a control test for the following tests for the
    /// <see cref="HttpMockServerBuilderExtensions.UseDefaultLogLevel"/> extension method.
    /// </summary>
    [Fact]
    public async Task UseDefaultLogLevelControlTest()
    {
        // The '.UseHostArgs("Logging:LogLevel:Microsoft.Hosting.Lifetime", LogLevel.None.ToString())'
        // disables test output logs from the server starting.
        // Usually I disable all logs by setting hte default log level to Critical but since I this is
        // a control test for the log level I'm only setting the log level for the source 'Microsoft.Hosting.Lifetime'
        // which is the source that outputs some logs on this test
        // LogLevel.None.ToString()

        await using var httpMockServer = new HttpMockServerBuilder()
            .UseHostArgs("--Logging:LogLevel:Microsoft.Hosting.Lifetime", nameof(LogLevel.None))
            .UseHttpResponseMocks()
            .Build();
        _ = await httpMockServer.StartAsync();

        var configuration = httpMockServer.Host!.Services.GetRequiredService<IConfiguration>();
        configuration["Logging:LogLevel:Default"].ShouldBeNull();
    }

    /// <summary>
    /// Tests that the <seealso cref="HttpMockServerBuilderExtensions.UseDefaultLogLevel"/>
    /// sets the default log level on the <see cref="IConfiguration"/>.
    /// </summary>
    [Fact]
    public async Task UseDefaultLogLevel()
    {
        await using var httpMockServer = new HttpMockServerBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .UseHttpResponseMocks()
            .Build();
        _ = await httpMockServer.StartAsync();

        var configuration = httpMockServer.Host!.Services.GetRequiredService<IConfiguration>();
        configuration["Logging:LogLevel:Default"].ShouldBe("Critical");
    }
}
