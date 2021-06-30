using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess;
using DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess.Auxiliary.Timeout;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess
{
    [Trait("Category", XUnitCategories.HttpMockingInProcess)]
    public class TimeoutTests : IClassFixture<TimeoutHttpResponseMockingWebApplicationFactory>, IDisposable
    {
        private readonly TimeoutHttpResponseMockingWebApplicationFactory _webApplicationFactory;

        public TimeoutTests(TimeoutHttpResponseMockingWebApplicationFactory webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        /// <summary>
        /// The setup for this sets up a named HttpClient "named-client" without further configuration.
        /// This tests that if we define a mock to timeout it will timeout as expected.
        /// </summary>
        [Fact]
        public async Task TimeoutOnHttpClientWithDefaultTimeout()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForNamedClient("named-client")
                                .TimesOut(TimeSpan.FromMilliseconds(50));
                        });
                    });
                })
                .CreateClient();

            // for some reason the exception returned by Should.ThrowAsync is missing the InnerException so
            // we are using the try/catch code as a workaround
            // I've raised an issue at https://github.com/shouldly/shouldly/issues/817
            TaskCanceledException? expectedException = null;
            try
            {
                await httpClient.GetAsync("/named-client");
            }
            catch (TaskCanceledException exception)
            {
                expectedException = exception;
            }

            expectedException.ShouldNotBeNull("Expected TaskCanceledException but didn't get any.");
            expectedException.ShouldBeOfType<TaskCanceledException>();
            expectedException.InnerException.ShouldBeOfType<TimeoutException>();
            expectedException.Message.ShouldBe("The request was canceled due to the configured HttpClient.Timeout of 0.05 seconds elapsing.");
            expectedException.InnerException.Message.ShouldBe("A task was canceled.");
        }

        /// <summary>
        /// The setup for this sets up a named HttpClient "named-client-with-timeout" and with a configured timeout of 200ms.
        /// This tests that if we define a mock to timeout for an HttpClient with an existing timeout configuration then
        /// the lowest timeout will be triggered first.
        ///
        /// In this test the timeout of 1 second defined on the mock is higher than the timeout of 200ms defined
        /// on the HttpClient.
        /// </summary>
        [Fact]
        public async Task TimeoutOnHttpClientWithTimeoutConfigured1()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForNamedClient("named-client-with-timeout")
                                .TimesOut(TimeSpan.FromSeconds(1));
                        });
                    });
                })
                .CreateClient();

            // for some reason the exception returned by Should.ThrowAsync is missing the InnerException so
            // we are using the try/catch code as a workaround
            // I've raised an issue at https://github.com/shouldly/shouldly/issues/817
            TaskCanceledException? expectedException = null;
            try
            {
                await httpClient.GetAsync("/named-client-with-timeout");
            }
            catch (TaskCanceledException exception)
            {
                expectedException = exception;
            }

            expectedException.ShouldNotBeNull("Expected TaskCanceledException but didn't get any.");
            expectedException.ShouldBeOfType<TaskCanceledException>();
            expectedException.InnerException.ShouldBeOfType<TimeoutException>();
            expectedException.Message.ShouldBe("The request was canceled due to the configured HttpClient.Timeout of 0.2 seconds elapsing.");
            expectedException.InnerException.Message.ShouldBe("A task was canceled.");
        }

        /// <summary>
        /// The setup for this sets up a named HttpClient "named-client-with-timeout" and with a configured timeout of 200ms.
        /// This tests that if we define a mock to timeout for an HttpClient with an existing timeout configuration then
        /// the lowest timeout will be triggered first.
        ///
        /// In this test the timeout of 1ms defined on the mock is lower than the timeout of 200ms defined
        /// on the HttpClient.
        /// </summary>
        [Fact]
        public async Task TimeoutOnHttpClientWithTimeoutConfigured2()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForNamedClient("named-client-with-timeout")
                                .TimesOut(TimeSpan.FromMilliseconds(1));
                        });
                    });
                })
                .CreateClient();

            // for some reason the exception returned by Should.ThrowAsync is missing the InnerException so
            // we are using the try/catch code as a workaround
            // I've raised an issue at https://github.com/shouldly/shouldly/issues/817
            TaskCanceledException? expectedException = null;
            try
            {
                await httpClient.GetAsync("/named-client-with-timeout");
            }
            catch (TaskCanceledException exception)
            {
                expectedException = exception;
            }

            expectedException.ShouldNotBeNull("Expected TaskCanceledException but didn't get any.");
            expectedException.ShouldBeOfType<TaskCanceledException>();
            expectedException.InnerException.ShouldBeOfType<TimeoutException>();
            expectedException.Message.ShouldBe("The request was canceled due to the configured HttpClient.Timeout of 0.001 seconds elapsing.");
            expectedException.InnerException.Message.ShouldBe("A task was canceled.");
        }


        /// <summary>
        /// The setup for this test uses Polly to define a timeout policy for the named HttpClient "polly-named-client".
        /// The timeout for the HttpClient is set to 200ms and the HttpClient is invoked when doing a GET to /polly-named-client.
        /// 
        /// This tests that if we define a mock to timeout for an HttpClient with an existing timeout configuration then
        /// the lowest timeout will be triggered first.
        ///
        /// In this test the timeout of 1 second defined on the mock is higher than the timeout of 200ms defined
        /// on the HttpClient so Polly throws a TimeoutRejectedException when a timeout occurs.
        /// </summary>
        [Fact]
        public async Task TimeoutWithPolly()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForNamedClient("polly-named-client")
                                .TimesOut(TimeSpan.FromSeconds(1));
                        });
                    });
                })
                .CreateClient();

            var exception = await Should.ThrowAsync<TimeoutRejectedException>(httpClient.GetAsync("/polly-named-client"));
            exception.Message.ShouldBe("The delegate executed asynchronously through TimeoutPolicy did not complete within the timeout.");
        }

        /// <summary>
        /// The setup for this test uses Polly to define a timeout policy for the named HttpClient "polly-named-client".
        /// The timeout for the HttpClient is set to 200ms and the HttpClient is invoked when doing a GET to /polly-named-client.
        ///
        /// This tests that if we define a mock to timeout for an HttpClient with an existing timeout configuration then
        /// the lowest timeout will be triggered first.
        ///
        /// In this test the timeout of 1ms defined on the mock is lower than the timeout of 200ms defined
        /// on the HttpClient.
        /// </summary>
        [Fact]
        public async Task TimeoutWithPolly2()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForNamedClient("polly-named-client")
                                .TimesOut(TimeSpan.FromMilliseconds(1));
                        });
                    });
                })
                .CreateClient();

            // for some reason the exception returned by Should.ThrowAsync is missing the InnerException so
            // we are using the try/catch code as a workaround
            // I've raised an issue at https://github.com/shouldly/shouldly/issues/817
            TaskCanceledException? expectedException = null;
            try
            {
                await httpClient.GetAsync("/polly-named-client");
            }
            catch (TaskCanceledException exception)
            {
                expectedException = exception;
            }

            expectedException.ShouldNotBeNull("Expected TaskCanceledException but didn't get any.");
            expectedException.ShouldBeOfType<TaskCanceledException>();
            expectedException.InnerException.ShouldBeOfType<TimeoutException>();
            expectedException.Message.ShouldBe("The request was canceled due to the configured HttpClient.Timeout of 0.001 seconds elapsing.");
            expectedException.InnerException.Message.ShouldBe("A task was canceled.");
        }

        public void Dispose()
        {
            _webApplicationFactory.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
