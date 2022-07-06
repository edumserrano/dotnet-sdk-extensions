namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;

/// <summary>
/// HTTP server to be used for tests when you want to mock out of process HTTP responses.
/// </summary>
public interface IHttpMockServer : IAsyncDisposable
{
    /// <summary>
    /// Gets the <see cref="IHost"/> used by the <see cref="HttpMockServer"/>.
    /// </summary>
    public IHost? Host { get; }

    /// <summary>
    /// Starts the server.
    /// </summary>
    /// <returns>The URLs where the server is listening for requests.</returns>
    Task<List<HttpMockServerUrl>> StartAsync();
}
