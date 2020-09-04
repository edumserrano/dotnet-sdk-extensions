using System;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    public class RunUntilException : Exception
    {
        internal RunUntilException(string message)
            : base(message)
        {
        }

        internal RunUntilException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}