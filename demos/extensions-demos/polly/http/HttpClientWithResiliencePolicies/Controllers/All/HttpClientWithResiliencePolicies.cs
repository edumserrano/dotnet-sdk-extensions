using System.Net.Http;

namespace HttpClientWithResiliencePolicies.Controllers.All
{
    public class HttpClientWithResiliencePolicies
    {
        private readonly HttpClient _httpClient;

        public HttpClientWithResiliencePolicies(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
