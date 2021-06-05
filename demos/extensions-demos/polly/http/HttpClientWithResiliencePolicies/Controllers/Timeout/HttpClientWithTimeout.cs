using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientWithResiliencePolicies.Controllers.Timeout
{
    public class HttpClientWithTimeout
    {
        private readonly HttpClient _httpClient;

        public HttpClientWithTimeout(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<HttpResponseMessage> DoSomeHttpOperationAsync()
        {
            return _httpClient.GetAsync("/");
        }
    }
}
