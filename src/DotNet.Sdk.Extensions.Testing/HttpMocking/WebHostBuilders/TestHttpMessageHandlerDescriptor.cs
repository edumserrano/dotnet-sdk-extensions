using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders
{
    internal class TestHttpMessageHandlerDescriptor
    {
        public TestHttpMessageHandlerDescriptor(string httpClientName, TestHttpMessageHandler httpMessageHandler)
        {
            HttpClientName = httpClientName;
            HttpMessageHandler = httpMessageHandler;
        }

        public string HttpClientName { get; }

        public TestHttpMessageHandler HttpMessageHandler { get; }
    }
}