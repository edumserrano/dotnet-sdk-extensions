using System.Collections.Generic;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased
{
    internal class ResponseBasedHttpMockServer : HttpMockServer
    {
        private readonly HttpResponseMocksProvider _httpResponseMocksProvider;

        public ResponseBasedHttpMockServer(HttpMockServerArgs mockServerArgs, ICollection<HttpResponseMock> httpResponseMocks) : base(mockServerArgs)
        {
            _httpResponseMocksProvider = new HttpResponseMocksProvider(httpResponseMocks);
        }

        protected override IHostBuilder CreateHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    _ = webBuilder.ConfigureServices(services =>
                      {
                          _ = services.AddSingleton(_httpResponseMocksProvider);
                      });
                    _ = webBuilder.UseStartup<HttpMockServerStartup>();
                });
        }
    }


}
