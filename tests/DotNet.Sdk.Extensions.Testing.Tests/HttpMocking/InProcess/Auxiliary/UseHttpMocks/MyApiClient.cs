namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess.Auxiliary.UseHttpMocks;

public class MyApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<bool> DoSomeHttpCall()
    {
        var response = await _httpClient.GetAsync("https://typed-client.com");
        return response.IsSuccessStatusCode;
    }
}
