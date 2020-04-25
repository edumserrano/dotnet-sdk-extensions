using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.Demos.TestApp.DemoStartups.HttpMocking
{
    public interface IMyApiClient
    {
        Task<bool> DoSomeHttpCall();
    }

    public class MyApiClient : IMyApiClient
    {
        private readonly HttpClient _httpClient;

        public MyApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> DoSomeHttpCall()
        {
            var response = await _httpClient.GetAsync("https://typed-client.com");
            return response.IsSuccessStatusCode;
        }
    }
}
