using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess.Auxiliary.Timeout
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Used as generic type param.")]
    internal class ExceptionService
    {
        private readonly List<Exception> _exceptions = new List<Exception>();

        public IReadOnlyCollection<Exception> Exceptions
        {
            get { return _exceptions; }
        }

        public void AddException(Exception exception)
        {
            _exceptions.Add(exception);
        }
    }
}
