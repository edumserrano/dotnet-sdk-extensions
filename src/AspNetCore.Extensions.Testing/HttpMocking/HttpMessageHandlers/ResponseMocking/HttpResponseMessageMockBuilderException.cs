using System;

namespace AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
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