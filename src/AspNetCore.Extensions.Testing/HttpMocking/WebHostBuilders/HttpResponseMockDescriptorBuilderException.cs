using System;

namespace AspNetCore.Extensions.Testing.HttpMocking.WebHostBuilders
{
    internal class HttpResponseMockDescriptorBuilderException : Exception
    {
        public HttpResponseMockDescriptorBuilderException(string message)
            : base(message)
        {
        }

        public HttpResponseMockDescriptorBuilderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}