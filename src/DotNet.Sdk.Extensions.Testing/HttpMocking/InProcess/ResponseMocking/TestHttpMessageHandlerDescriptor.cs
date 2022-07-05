using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess.ResponseMocking;

internal class TestHttpMessageHandlerDescriptor
{
    public TestHttpMessageHandlerDescriptor(string httpClientName, TestHttpMessageHandler httpMessageHandler)
    {
        HttpClientName = httpClientName;
        HttpMessageHandler = httpMessageHandler ?? throw new ArgumentNullException(nameof(httpMessageHandler));
    }

    public string HttpClientName { get; }

    public TestHttpMessageHandler HttpMessageHandler { get; }
}
