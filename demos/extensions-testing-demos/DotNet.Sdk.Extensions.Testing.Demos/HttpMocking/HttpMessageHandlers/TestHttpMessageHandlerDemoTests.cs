using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.HttpMessageHandlers.Auxiliary;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.HttpMessageHandlers
{
    /*
     * This shows how to use the TestHttpMessageHandler to unit test classes that use an HttpClient.
     *
     * The MyAwesomeOutboundDependency class takes in an HttpClient as a dependency and this shows how to use the
     * TestHttpMessageHandler to mock the responses returned by the HttpClient during tests so that you can
     * unit test the MyAwesomeOutboundDependency class.
     */
    public class TestHttpMessageHandlerDemoTests
    {
        [Fact]
        public async Task MockHttpResponseExample1()
        {
            // prepare the http mocks
            var httpResponseMessageMock = new HttpResponseMessageMockBuilder()
                .Where(httpRequestMessage =>
                {
                    return httpRequestMessage.Method == HttpMethod.Get &&
                        httpRequestMessage.RequestUri.PathAndQuery.Equals("/some-http-call");
                })
                .RespondWith(httpRequestMessage =>
                {
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Created);
                    httpResponseMessage.Content = new StringContent("mocked value");
                    return httpResponseMessage;
                })
                .Build();

            // add the mocks to the http handler
            var handler = new TestHttpMessageHandler();
            handler.MockHttpResponse(httpResponseMessageMock);

            // instantiate the http client with the test handler
            var httpClient = new HttpClient(handler);
            var sut = new MyAwesomeOutboundDependency(httpClient);

            // the sut.DoSomeHttpCall method call will do a GET request to the path /some-http-call
            // so it will match our mock conditions defined above and the mock response will be returned
            var response = await sut.DoSomeHttpCall(); 
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.ShouldBe("mocked value");
        }

        /*
         * This is the same as MockHttpResponseExample1 test but shows that you can configure
         * the mock inline with the TestHttpMessageHandler.MockHttpResponse method call
         * as opposed to configuring it before hand
         *
         */
        [Fact]
        public async Task MockHttpResponseExample2()
        {
            // configure the http handler with the desired http mocks
            var handler = new TestHttpMessageHandler();
            handler.MockHttpResponse(builder =>
            {
                builder.Where(httpRequestMessage => 
                {
                    return httpRequestMessage.Method == HttpMethod.Get &&
                           httpRequestMessage.RequestUri.PathAndQuery.Equals("/some-http-call");
                })
                .RespondWith(httpRequestMessage =>
                {
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Created);
                    httpResponseMessage.Content = new StringContent("mocked value");
                    return httpResponseMessage;
                });
            });

            // instantiate the http client with the test handler
            var httpClient = new HttpClient(handler);
            var sut = new MyAwesomeOutboundDependency(httpClient);

            // the sut.DoSomeHttpCall method call will do a GET request to the path /some-http-call
            // so it will match our mock conditions defined above and the mock response will be returned
            var response = await sut.DoSomeHttpCall();
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.ShouldBe("mocked value");
        }

        [Fact]
        public async Task MockMultipleHttpResponses()
        {
            // prepare the http mocks
            var someHttpCallMock = new HttpResponseMessageMockBuilder()
                .Where(httpRequestMessage => httpRequestMessage.RequestUri.PathAndQuery.Equals("/some-http-call"))
                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("some mocked value")
                })
                .Build();
            var anotherHttpCallMock = new HttpResponseMessageMockBuilder()
                .Where(httpRequestMessage => httpRequestMessage.RequestUri.PathAndQuery.Equals("/another-http-call"))
                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.Accepted)
                {
                    Content = new StringContent("another mocked value")
                })
                .Build();

            // add the mocks to the http handler
            var handler = new TestHttpMessageHandler();
            handler
                .MockHttpResponse(someHttpCallMock)
                .MockHttpResponse(anotherHttpCallMock);

            // instantiate the http client with the test handler
            var httpClient = new HttpClient(handler);
            var sut = new MyAwesomeOutboundDependency(httpClient);
            
            // the sut.DoSomeHttpCall method call will do a GET request to the path /some-http-call
            // so it will match one the mock conditions defined above and the correct mock response will be returned
            var someResponse = await sut.DoSomeHttpCall();
            someResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
            var someResponseBody = await someResponse.Content.ReadAsStringAsync();
            someResponseBody.ShouldBe("some mocked value");


            // the sut.DoAnotherHttpCall method call will do a GET request to the path /another-http-call
            // so it will match one of the mock conditions defined above and the correct mock response will be returned
            var anotherResponse = await sut.DoAnotherHttpCall();
            anotherResponse.StatusCode.ShouldBe(HttpStatusCode.Accepted);
            var anotherResponseBody = await anotherResponse.Content.ReadAsStringAsync();
            anotherResponseBody.ShouldBe("another mocked value");
        }

        [Fact]
        public async Task MockTimeOut()
        {
            // Configure the http handler with the desired http mocks.
            // Since no where clause is defined the default condition is applied.
            // The default where clause will match for ALL requests.
            // Meaning that the below configuration will make any http requests will timeout after 1 ms.
            var handler = new TestHttpMessageHandler()
                .MockHttpResponse(builder => builder.TimesOut(TimeSpan.FromMilliseconds(1)));

            // instantiate the http client with the test handler
            var httpClient = new HttpClient(handler);
            var sut = new MyAwesomeOutboundDependency(httpClient);

            // show that the http call will timeout
            Exception? expectedException = null;
            try
            {
                await sut.DoAnotherHttpCall();
            }
            catch (Exception exception)
            {
                expectedException = exception;
            }
            
            // show that you get the expected timeout exception
            expectedException!.GetType().ShouldBe(typeof(TaskCanceledException));
            expectedException.InnerException!.GetType().ShouldBe(typeof(TimeoutException));
            expectedException.Message.ShouldBe("The request was canceled due to the configured HttpClient.Timeout of 0.001 seconds elapsing.");
            expectedException.InnerException.Message.ShouldBe("A task was canceled.");
        }
    }
}
