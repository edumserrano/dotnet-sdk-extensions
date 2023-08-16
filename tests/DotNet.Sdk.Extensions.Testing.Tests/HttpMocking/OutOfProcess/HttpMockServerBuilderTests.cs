using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.OutOfProcess;

[Trait("Category", XUnitCategories.HttpMockingOutOfProcess)]
public class HttpMockServerBuilderTests
{
    /// <summary>
    /// Tests that by default, if no URL is provided, <see cref="HttpMockServer.StartAsync"/>
    /// starts the server in two URLs, one HTTP another HTTPS.
    /// </summary>
    [Fact]
    public async Task ProvidesTwoUrlsByDefault()
    {
        await using var httpMockServer = new HttpMockServerBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .UseHttpResponseMocks()
            .Build();
        var urls = await httpMockServer.StartAsync();

        urls.Count.ShouldBe(2);
        urls[0].Scheme.ShouldBe(HttpScheme.Http);
        urls[0].Host.ShouldBe("localhost");
        urls[1].Scheme.ShouldBe(HttpScheme.Https);
        urls[1].Host.ShouldBe("localhost");
    }

    /// <summary>
    /// Tests that <see cref="HttpMockServer"/> can only be started once.
    /// </summary>
    [Fact]
    public async Task RepliesAsConfigured()
    {
        await using var httpMockServer = new HttpMockServerBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .UseHttpResponseMocks()
            .Build();
        await httpMockServer.StartAsync();
        var exception = await Should.ThrowAsync<InvalidOperationException>(httpMockServer.StartAsync());
        exception.Message.ShouldBe("The HttpMockServer has already been started.");
    }

    /// <summary>
    /// Tests that the <see cref="HttpMockServer.StartAsync"/> starts the server using the provided URLs
    /// by <see cref="HttpMockServerBuilder.UseUrls"/>.
    /// </summary>
    [Fact]
    public async Task AllowsMultipleUrlsToBeConfigured()
    {
        var runtimeMajorVersion = Environment.Version.Major;
        await using var httpMockServer = new HttpMockServerBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .UseUrls(HttpScheme.Http, 9020 + runtimeMajorVersion)
            .UseUrls(HttpScheme.Http, 9030 + runtimeMajorVersion)
            .UseUrls(HttpScheme.Https, 9040 + runtimeMajorVersion)
            .UseUrls(HttpScheme.Https, 9060 + runtimeMajorVersion)
            .UseHttpResponseMocks()
            .Build();
        var urls = await httpMockServer.StartAsync();

        urls.Count.ShouldBe(4);
        urls[0].ToString().ShouldBe($"http://localhost:{9020 + runtimeMajorVersion}");
        urls[1].ToString().ShouldBe($"http://localhost:{9030 + runtimeMajorVersion}");
        urls[2].ToString().ShouldBe($"https://localhost:{9040 + runtimeMajorVersion}");
        urls[3].ToString().ShouldBe($"https://localhost:{9060 + runtimeMajorVersion}");
    }

    /// <summary>
    /// Tests that the <see cref="HttpMockServer.StartAsync"/> starts the server using the provided
    /// host args by <see cref="HttpMockServerBuilder.UseHostArgs"/>.
    /// This is testing the --urls <see cref="IHost"/> command line argument but you can test with
    /// any other.
    /// </summary>
    [Fact]
    public async Task UsesHostArgs()
    {
        var runtimeMajorVersion = Environment.Version.Major;
        await using var httpMockServer = new HttpMockServerBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .UseHostArgs("--urls", $"http://*:{9070 + runtimeMajorVersion};https://*:{9080 + runtimeMajorVersion}")
            .UseHttpResponseMocks()
            .Build();
        var urls = await httpMockServer.StartAsync();

        urls.Count.ShouldBe(2);
        urls[0].ToString().ShouldBe($"http://localhost:{9070 + runtimeMajorVersion}");
        urls[1].ToString().ShouldBe($"https://localhost:{9080 + runtimeMajorVersion}");
    }

    /// <summary>
    /// Tests that <see cref="HttpMockServerBuilder.UseHostArgs"/> cannot be null.
    /// </summary>
    [Fact]
    public void UsesHostArgsCannotBeNull()
    {
        var exception = Should.Throw<ArgumentNullException>(() =>
        {
            new HttpMockServerBuilder()
                .UseHostArgs(null!);
        });
        exception.Message.ShouldBe("Value cannot be null. (Parameter 'hostArgs')");
    }

    /// <summary>
    /// Tests that <see cref="HttpMockServerBuilder.UseHostArgs"/> must have a value.
    /// </summary>
    [Fact]
    public void UsesHostArgsMustHaveAValue()
    {
        var exception = Should.Throw<ArgumentException>(() =>
        {
            new HttpMockServerBuilder()
                .UseHostArgs();
        });
        exception.Message.ShouldBe("Must have a value. (Parameter 'hostArgs')");
    }

    /// <summary>
    /// Tests that <see cref="HttpMockServerBuilder.UseHostArgs"/> can be defined multiple times.
    /// </summary>
    [Fact]
    public async Task UsesHostArgsCanBeRepeated()
    {
        await using var httpMockServer = new HttpMockServerBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .UseHostArgs("--config1", "value1")
            .UseHostArgs("--config2", "value2")
            .UseHttpResponseMocks()
            .Build();
        _ = await httpMockServer.StartAsync();

        var configuration = httpMockServer.Host!.Services.GetRequiredService<IConfiguration>();
        configuration["config1"].ShouldBe("value1");
        configuration["config2"].ShouldBe("value2");
    }

    /// <summary>
    /// Tests that the <see cref="HttpMockServer.StartAsync"/> fails to start if there are
    /// competing URLs configurations.
    /// </summary>
    [Fact]
    public void CompetingUrlsConfigurations()
    {
        var runtimeMajorVersion = Environment.Version.Major;
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            new HttpMockServerBuilder()
                .UseUrls(HttpScheme.Http, 7777)
                .UseHostArgs("--urls", $"http://*:{9180 + runtimeMajorVersion};https://*:{9190 + runtimeMajorVersion}")
                .UseHttpResponseMocks()
                .Build();
        });
        exception.Message.ShouldBe("Competing URLs configuration. URls defined via both HttpMockServerBuilder.UseUrls method and by defining an '--urls' arg via HttpMockServerBuilder.UseHostArgs. Use only one of these methods to configure the URLs.");
    }
}
