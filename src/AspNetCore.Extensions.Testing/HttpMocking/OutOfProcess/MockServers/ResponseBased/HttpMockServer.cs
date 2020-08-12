using System.Collections.Generic;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased
{
    public class HttpMockServer : HttpMockServerBase
    {
        private readonly HttpResponseMocksProvider _httpResponseMocksProvider;

        public HttpMockServer(ICollection<HttpResponseMock> httpResponseMocks)
        {
            _httpResponseMocksProvider = new HttpResponseMocksProvider(httpResponseMocks);
        }

        public override IHostBuilder CreateHostBuilder(string[] args)
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
