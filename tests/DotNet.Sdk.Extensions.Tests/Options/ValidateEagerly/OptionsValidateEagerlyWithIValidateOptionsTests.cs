namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly;

[Trait("Category", XUnitCategories.Options)]
public class OptionsValidateEagerlyWithIValidateOptionsTests
{
    /// <summary>
    /// This tests that the <see cref="IHost"/> will throw an exception when starting if the options
    /// validation fails.
    /// </summary>
    [Fact]
    public async Task ServerFailsToStartIfOptionsValidationFails()
    {
        using var host = Host
            .CreateDefaultBuilder()
            .UseDefaultLogLevel(LogLevel.None) // expect critical error log so disabling all logs
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .UseLocalhostWithRandomPort()
                    .UseStartup<StartupMyOptions1ValidateEagerly>();
            })
            .Build();
        var validationException = await Should.ThrowAsync<ValidationException>(host.StartAsync());
        validationException.Message.ShouldBe("The SomeOption field is required.");
    }

    /// <summary>
    /// This tests that the <see cref="IHost"/> will start as expected if the options validation succeeds.
    /// The validation is checking that the MyOptions1.SomeOption is not null or empty.
    /// </summary>
    [Fact]
    public void ServerStartsIfOptionsValidationSucceeds()
    {
        using var host = Host
            .CreateDefaultBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .ConfigureAppConfiguration((_, builder) =>
            {
                var memoryConfigurationSource = new MemoryConfigurationSource
                {
                    InitialData = new List<KeyValuePair<string, string?>>
                    {
                        new KeyValuePair<string, string?>("SomeOption", "some value"),
                    },
                };
                builder.Add(memoryConfigurationSource);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .UseLocalhostWithRandomPort()
                    .UseStartup<StartupMyOptions1ValidateEagerly>();
            })
            .Build();
        Should.NotThrow(async () => await host.StartAsync());
    }
}
