using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace AspNetCore.Extensions.Testing.HttpMocking
{
    public static class HttpMessageHandlersBuilderExtensions
    {
        public static IWebHostBuilder UseHttpMocks(this IWebHostBuilder webHostBuilder, Action<HttpMessageHandlersBuilder> configure)
        {
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