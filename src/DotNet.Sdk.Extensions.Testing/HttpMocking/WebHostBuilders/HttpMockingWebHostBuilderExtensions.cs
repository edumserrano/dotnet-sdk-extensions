using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders
{
    public static class HttpMockingWebHostBuilderExtensions
    {
        public static IWebHostBuilder UseHttpMocks(this IWebHostBuilder webHostBuilder, Action<HttpMessageHandlersBuilder> configure)
        {
            if (webHostBuilder == null) throw new ArgumentNullException(nameof(webHostBuilder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            webHostBuilder.ConfigureTestServices(services =>
            {
                var builder = new HttpMessageHandlersBuilder(services);
                configure(builder);
                builder.ApplyHttpResponseMocks();
            });
            return webHostBuilder;
        }
    }
}