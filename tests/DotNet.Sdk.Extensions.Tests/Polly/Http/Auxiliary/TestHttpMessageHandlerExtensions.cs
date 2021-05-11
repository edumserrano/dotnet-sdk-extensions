using System;
using System.Net;
using System.Net.Http;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary
{
    public static class TestHttpMessageHandlerExtensions
    {
        public static string HandleTransientHttpStatusCode(
            this TestHttpMessageHandler testHttpMessageHandler,
            string requestPath,
            HttpStatusCode responseHttpStatusCode)
        {
            var handledRequestPath = $"{requestPath}/{responseHttpStatusCode}";
            testHttpMessageHandler.MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains(handledRequestPath))
                    .RespondWith(new HttpResponseMessage(responseHttpStatusCode));
            });
            return handledRequestPath;
        }

        public static void HandleException(
            this TestHttpMessageHandler testHttpMessageHandler,
            string requestPath,
            Exception exception)
        {
            testHttpMessageHandler.MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains(requestPath))
                    .RespondWith(httpRequestMessage => throw exception);
            });
        }
    }
}
