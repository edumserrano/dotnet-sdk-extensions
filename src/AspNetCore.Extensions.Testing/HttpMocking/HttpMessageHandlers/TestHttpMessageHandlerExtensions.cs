using System;

namespace AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers
{
    public static class TestHttpMessageHandlerExtensions
    {
        public static TestHttpMessageHandler MockHttpResponse(
            this TestHttpMessageHandler testHttpMessageHandler,
            Action<HttpResponseMessageMockBuilder> configure)
        {
            var httpResponseMockBuilder = new HttpResponseMessageMockBuilder();
            configure(httpResponseMockBuilder);
            var httpResponseMock = httpResponseMockBuilder.Build();
            testHttpMessageHandler.MockHttpResponse(httpResponseMock);
            return testHttpMessageHandler;
        }
    }
}
