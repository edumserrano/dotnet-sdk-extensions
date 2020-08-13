using System;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking
{
    internal class HttpResponseMockBuilderException : Exception
    {
        public HttpResponseMockBuilderException(string message)
            : base(message)
        {
        }

        public HttpResponseMockBuilderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}