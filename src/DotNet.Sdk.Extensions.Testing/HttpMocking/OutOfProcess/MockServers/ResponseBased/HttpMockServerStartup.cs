using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased
{
    internal class HttpMockServerStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            _ = services
                .AddSingleton<DefaultResponseMiddleware>()
                .AddSingleton<ResponseMocksMiddleware>();

        }

        public void Configure(IApplicationBuilder app)
        {
            _ = app
                .UseResponseMocks()
                .RunDefaultResponse();
        }
    }
}