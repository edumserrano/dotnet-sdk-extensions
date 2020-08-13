using System.Collections.Generic;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased
{
    public class HttpMockServer : HttpMockServerBase
    {
        private readonly HttpResponseMocksProvider _httpResponseMocksProvider;

        public HttpMockServer(HttpMockServerArgs mockServerArgs, ICollection<HttpResponseMock> httpResponseMocks) : base(mockServerArgs)
        {
            _httpResponseMocksProvider = new HttpResponseMocksProvider(httpResponseMocks);
        }

        protected override IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddSingleton(_httpResponseMocksProvider);
                    });
                    webBuilder.UseStartup<HttpMockServerStartup>();
                });
        }
    }

    
}
