using System.Net.Http;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.HttpMessageHandlers.Auxiliary
{
    public class MyAwesomeOutboundDependency
    {
        private readonly HttpClient _httpClient;

        public MyAwesomeOutboundDependency(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> DoSomeHttpCall()
        {
            var httpResponseMessage = await _httpClient.GetAsync("https://thing.com/some-http-call");
            return httpResponseMessage;
        }

        public async Task<HttpResponseMessage> DoAnotherHttpCall()
        {
            var httpResponseMessage = await _httpClient.GetAsync("https://thing.com/another-http-call");
            return httpResponseMessage;
        }
    }
}
