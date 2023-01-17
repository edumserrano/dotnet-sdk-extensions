namespace DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess.ResponseMocking;

internal sealed class TestHttpMessageHandlerDescriptor
{
    public TestHttpMessageHandlerDescriptor(string httpClientName, TestHttpMessageHandler httpMessageHandler)
    {
        HttpClientName = httpClientName;
        HttpMessageHandler = httpMessageHandler ?? throw new ArgumentNullException(nameof(httpMessageHandler));
    }

    public string HttpClientName { get; }

    public TestHttpMessageHandler HttpMessageHandler { get; }
}
