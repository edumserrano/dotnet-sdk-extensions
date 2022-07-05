using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;

/// <summary>
/// Defines the properties that represent the outcome of executing a <see cref="HttpResponseMessageMock"/>.
/// </summary>
public sealed class HttpResponseMessageMockResult
{
    private HttpResponseMessage? _httpResponseMessage;

    private HttpResponseMessageMockResult()
    {
    }

    /// <summary>
    /// Gets the result of executing the <see cref="HttpResponseMessageMock"/>.
    /// </summary>
    public HttpResponseMessageMockResults Status { get; private set; }

    /// <summary>
    /// Gets the <see cref="HttpResponseMessage"/> produced by the <see cref="HttpResponseMessageMock"/>.
    /// </summary>
    public HttpResponseMessage HttpResponseMessage
    {
        get
        {
            if (Status != HttpResponseMessageMockResults.Executed)
            {
                throw new InvalidOperationException($"Cannot retrieve {nameof(HttpResponseMessage)} unless Status is {HttpResponseMockResults.Executed}. Status is {Status}.");
            }

            return _httpResponseMessage!;
        }
        private set => _httpResponseMessage = value;
    }

    internal static HttpResponseMessageMockResult Executed(HttpResponseMessage httpResponseMessage)
    {
        return new HttpResponseMessageMockResult
        {
            Status = HttpResponseMessageMockResults.Executed,
            HttpResponseMessage = httpResponseMessage,
        };
    }

    internal static HttpResponseMessageMockResult Skipped()
    {
        return new HttpResponseMessageMockResult
        {
            Status = HttpResponseMessageMockResults.Skipped,
        };
    }
}
