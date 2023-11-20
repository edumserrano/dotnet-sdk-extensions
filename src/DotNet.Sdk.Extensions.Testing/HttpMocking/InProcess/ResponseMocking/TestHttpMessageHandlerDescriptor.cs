namespace DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess.ResponseMocking;

internal sealed class TestHttpMessageHandlerDescriptor(string httpClientName, TestHttpMessageHandler httpMessageHandler)
{
    public string HttpClientName { get; } = httpClientName;

    public TestHttpMessageHandler HttpMessageHandler { get; } = httpMessageHandler ?? throw new ArgumentNullException(nameof(httpMessageHandler));
}
