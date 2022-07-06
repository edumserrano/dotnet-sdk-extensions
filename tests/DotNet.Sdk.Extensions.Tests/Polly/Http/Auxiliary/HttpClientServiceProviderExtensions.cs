namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;

public static class HttpClientServiceProviderExtensions
{
    public static HttpClient InstantiateNamedHttpClient(this ServiceProvider serviceProvider, string name)
    {
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        return httpClientFactory.CreateClient(name);
    }
}
