namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;

internal sealed class NumberOfCallsDelegatingHandler : DelegatingHandler
{
    public int NumberOfHttpRequests { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        NumberOfHttpRequests++;
        return base.SendAsync(request, cancellationToken);
    }

    public void Reset()
    {
        NumberOfHttpRequests = 0;
    }
}
