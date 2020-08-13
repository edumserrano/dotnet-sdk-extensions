using System;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    internal class HttpMockServerException : Exception
    {
        public HttpMockServerException(string message)
            : base(message)
        {
        }

        public HttpMockServerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}