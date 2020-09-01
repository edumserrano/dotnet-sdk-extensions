using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders
{
    public static class HttpMockingWebHostBuilderExtensions
    {
        /// <summary>
        /// Allows mocking <see cref="HttpClient"/> calls when the <see cref="HttpClient"/> used was created by the <see cref="IHttpClientFactory"/>.
        /// </summary>
        /// <param name="webHostBuilder">The <see cref="IWebHostBuilder"/> to introduce the mocks to.</param>
        /// <param name="configure">An action to configure an <see cref="HttpResponseMessage"/> mock.</param>
        /// <returns>The <see cref="IWebHostBuilder"/> for chaining.</returns>
        public static IWebHostBuilder UseHttpMocks(this IWebHostBuilder webHostBuilder, Action<HttpMessageHandlers> configure)
        {
            if (webHostBuilder == null) throw new ArgumentNullException(nameof(webHostBuilder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            
            webHostBuilder.ConfigureTestServices(services =>
            {
                var builder = new HttpMessageHandlers(services);
                configure(builder);
                builder.ApplyHttpResponseMocks();
            });
            return webHostBuilder;
        }
    }
}