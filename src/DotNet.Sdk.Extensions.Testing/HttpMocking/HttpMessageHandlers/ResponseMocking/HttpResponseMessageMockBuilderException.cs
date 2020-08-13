using System;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    internal class HttpResponseMessageMockBuilderException : Exception
    {
        public HttpResponseMessageMockBuilderException(string message)
            : base(message)
        {
        }

        public HttpResponseMessageMockBuilderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}