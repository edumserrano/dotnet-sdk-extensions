namespace DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess.ResponseMocking;

internal sealed class HttpResponseMessageMockDescriptor
{
    private HttpResponseMessageMockDescriptor(
        HttpResponseMessageMockTypes httpResponseMockType,
        string httpClientName,
        InProcessHttpResponseMessageMockBuilder httpResponseMessageMockBuilder)
    {
        ArgumentNullException.ThrowIfNull(httpResponseMessageMockBuilder);

        HttpResponseMockType = httpResponseMockType;
        HttpClientName = httpClientName;
        HttpResponseMock = httpResponseMessageMockBuilder.Build();
    }

    public HttpResponseMessageMockTypes HttpResponseMockType { get; }

    public string HttpClientName { get; }

    public HttpResponseMessageMock HttpResponseMock { get; }

    public static HttpResponseMessageMockDescriptor Typed(
        Type httpClientType,
        string name,
        InProcessHttpResponseMessageMockBuilder httpResponseMessageMockBuilder)
    {
        var httpClientName = string.IsNullOrWhiteSpace(name) ? httpClientType.Name : name;
        return new HttpResponseMessageMockDescriptor(
            HttpResponseMessageMockTypes.TypedClient,
            httpClientName,
            httpResponseMessageMockBuilder);
    }

    public static HttpResponseMessageMockDescriptor Named(
        string httpClientName,
        InProcessHttpResponseMessageMockBuilder httpResponseMessageMockBuilder)
    {
        return new HttpResponseMessageMockDescriptor(
            HttpResponseMessageMockTypes.NamedClient,
            httpClientName,
            httpResponseMessageMockBuilder);
    }

    public static HttpResponseMessageMockDescriptor Basic(InProcessHttpResponseMessageMockBuilder httpResponseMessageMockBuilder)
    {
        return new HttpResponseMessageMockDescriptor(
            HttpResponseMessageMockTypes.Basic,
            string.Empty,
            httpResponseMessageMockBuilder);
    }
}
