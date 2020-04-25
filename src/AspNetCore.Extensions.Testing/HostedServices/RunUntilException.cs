using System;

namespace AspNetCore.Extensions.Testing.HostedServices
{
    public class RunUntilException : Exception
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