using AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers;

namespace AspNetCore.Extensions.Testing.HttpMocking.WebHostBuilders
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