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
            if (testHttpMessageHandler is null)
            {
                throw new ArgumentNullException(nameof(testHttpMessageHandler));
            }

            var handledRequestPath = $"{requestPath}/{responseHttpStatusCode}";
            testHttpMessageHandler.MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains(handledRequestPath, StringComparison.OrdinalIgnoreCase))
                    .RespondWith(new HttpResponseMessage(responseHttpStatusCode));
            });
            return handledRequestPath;
        }

        public static void HandleException(
            this TestHttpMessageHandler testHttpMessageHandler,
            string requestPath,
            Exception exception)
        {
            if (testHttpMessageHandler is null)
            {
                throw new ArgumentNullException(nameof(testHttpMessageHandler));
            }

            testHttpMessageHandler.MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains(requestPath, StringComparison.OrdinalIgnoreCase))
                    .RespondWith(_ => throw exception);
            });
        }

        public static void HandleTimeout(
            this TestHttpMessageHandler testHttpMessageHandler,
            string requestPath,
            TimeSpan timeout)
        {
            if (testHttpMessageHandler is null)
            {
                throw new ArgumentNullException(nameof(testHttpMessageHandler));
            }

            testHttpMessageHandler.MockHttpResponse(builder =>
            {
                // this timeout is a max timeout before aborting but the polly timeout policy
                // will timeout before this happens
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains(requestPath, StringComparison.OrdinalIgnoreCase))
                    .TimesOut(timeout);
            });
        }
    }
}
