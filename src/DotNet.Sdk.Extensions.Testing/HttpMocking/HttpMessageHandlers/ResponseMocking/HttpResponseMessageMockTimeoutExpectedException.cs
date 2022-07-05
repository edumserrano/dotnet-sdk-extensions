namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;

/// <summary>
/// Exception thrown when an a timeout should have occured but didnt when mocking an HTTP response.
/// </summary>
public class TimeoutExpectedException : Exception
{
    internal TimeoutExpectedException(string message)
        : base(message)
    {
    }

    internal TimeoutExpectedException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
