using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.OutOfProcess
{
    public class HttpMockServerBuilderTests
    {
        /// <summary>
        /// Tests that by default, if no URL is provided, <seealso cref="HttpMockServer.StartAsync"/>
        /// starts the server in two URLs, one HTTP another HTTPS.
        /// </summary>
        [Fact]
        public async Task ProvidesTwoUrlsByDefault()
        {
            await using var mock = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHttpResponseMocks()
                .Build();
            var urls = await mock.StartAsync();

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
            await using var mock = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHttpResponseMocks()
                .Build();
            await mock.StartAsync();
            var exception = await Should.ThrowAsync<InvalidOperationException>(mock.StartAsync());
            exception.Message.ShouldBe("The HttpMockServer has already been started.");
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpMockServer.StartAsync"/> starts the server using the provided URLs
        /// by <seealso cref="HttpMockServerBuilder.UseUrl"/>
        /// </summary>
        [Fact]
        public async Task AllowsMultipleUrlsToBeConfigured()
        {
            await using var mock = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseUrl(HttpScheme.Http, 8811)
                .UseUrl(HttpScheme.Http, 8822)
                .UseUrl(HttpScheme.Https, 9911)
                .UseUrl(HttpScheme.Https, 9922)
                .UseHttpResponseMocks()
                .Build();
            var urls = await mock.StartAsync();

            urls.Count.ShouldBe(4);
            urls[0].ToString().ShouldBe("http://localhost:8811");
            urls[1].ToString().ShouldBe("http://localhost:8822");
            urls[2].ToString().ShouldBe("https://localhost:9911");
            urls[3].ToString().ShouldBe("https://localhost:9922");
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpMockServer.StartAsync"/> starts the server using the provided
        /// host args by <seealso cref="HttpMockServerBuilder.UseHostArgs"/>.
        /// This is testing the --urls <seealso cref="IHost"/> command line argument but you can test with
        /// any other.
        /// </summary>
        [Fact]
        public async Task UsesHostArgs()
        {
            await using var mock = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHostArgs("--urls", "http://*:8811;https://*:9911")
                .UseHttpResponseMocks()
                .Build();
            var urls = await mock.StartAsync();

            urls.Count.ShouldBe(2);
            urls[0].ToString().ShouldBe("http://localhost:8811");
            urls[1].ToString().ShouldBe("https://localhost:9911");
        }

        /// <summary>
        /// Tests that <seealso cref="HttpMockServerBuilder.UseHostArgs"/> cannot be null.
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
        /// Tests that <seealso cref="HttpMockServerBuilder.UseHostArgs"/> must have a value.
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
        /// Tests that <seealso cref="HttpMockServerBuilder.UseHostArgs"/> can be defined multiple times.
        /// </summary>
        [Fact]
        public async Task UsesHostArgsCanBeRepeated()
        {
            await using var mock = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHostArgs("--config1","value1")
                .UseHostArgs("--config2","value2")                                      
                .UseHttpResponseMocks()
                .Build();
            var urls = await mock.StartAsync();

            var configuration = mock.Host!.Services.GetRequiredService<IConfiguration>();
            configuration["config1"].ShouldBe("value1");
            configuration["config2"].ShouldBe("value2");
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpMockServer.StartAsync"/> fails to start if there are
        /// competing URLs configurations.
        /// </summary>
        [Fact]
        public void CompetingUrlsConfigurations()
        {
            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                new HttpMockServerBuilder()
                    .UseUrl(HttpScheme.Http, 7777)
                    .UseHostArgs("--urls", "http://*:8811;https://*:9911")
                    .UseHttpResponseMocks()
                    .Build();
            });
            exception.Message.ShouldBe("Competing URLs configuration. URls defined via both HttpMockServerBuilder.UseUrl method and by defining an '--urls' arg via HttpMockServerBuilder.UseHostArgs. Use only one of these methods to configure the URLs.");
        }
    }
}
