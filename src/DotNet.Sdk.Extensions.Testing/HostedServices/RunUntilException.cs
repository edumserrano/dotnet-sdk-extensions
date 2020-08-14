using System;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    internal class RunUntilException : Exception
    {
        public RunUntilException(string message)
            : base(message)
        {
        }

        public RunUntilException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}