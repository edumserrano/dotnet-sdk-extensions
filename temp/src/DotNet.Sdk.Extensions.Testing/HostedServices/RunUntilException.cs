using System;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    /// <summary>
    /// Exception thrown when an error occurs when using the RunUntil extension methods.
    /// </summary>
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
