namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased;

internal sealed class HttpResponseMocksProvider
{
    public HttpResponseMocksProvider(IReadOnlyCollection<HttpResponseMock> httpResponseMocks)
    {
        HttpResponseMocks = httpResponseMocks ?? throw new ArgumentNullException(nameof(httpResponseMocks));
    }

    public IReadOnlyCollection<HttpResponseMock> HttpResponseMocks { get; }
}
