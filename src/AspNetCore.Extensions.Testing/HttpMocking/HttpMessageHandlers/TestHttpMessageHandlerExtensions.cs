using System;

namespace AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers
{
    public static class TestHttpMessageHandlerExtensions
    {
        public static TestHttpMessageHandler MockHttpResponse(
            this TestHttpMessageHandler testHttpMessageHandler,
            Action<HttpResponseMockBuilder> configure)
        {
            var httpResponseMockBuilder = new HttpResponseMockBuilder();
            configure(httpResponseMockBuilder);
            var httpResponseMock = httpResponseMockBuilder.Build();
            testHttpMessageHandler.MockHttpResponse(httpResponseMock);
            return testHttpMessageHandler;
        }
    }
}
