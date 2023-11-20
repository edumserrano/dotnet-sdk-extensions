namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased;

internal sealed class HttpResponseMocksProvider(IReadOnlyCollection<HttpResponseMock> httpResponseMocks)
{
    public IReadOnlyCollection<HttpResponseMock> HttpResponseMocks { get; } = httpResponseMocks ?? throw new ArgumentNullException(nameof(httpResponseMocks));
}
