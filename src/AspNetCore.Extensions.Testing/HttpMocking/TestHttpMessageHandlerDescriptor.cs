namespace AspNetCore.Extensions.Testing.HttpMocking
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