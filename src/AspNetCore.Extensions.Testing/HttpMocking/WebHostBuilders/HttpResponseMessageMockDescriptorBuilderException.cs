using System;

namespace AspNetCore.Extensions.Testing.HttpMocking.WebHostBuilders
{
    internal class HttpResponseMessageMockDescriptorBuilderException : Exception
    {
        public HttpResponseMessageMockDescriptorBuilderException(string message)
            : base(message)
        {
        }

        public HttpResponseMessageMockDescriptorBuilderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}