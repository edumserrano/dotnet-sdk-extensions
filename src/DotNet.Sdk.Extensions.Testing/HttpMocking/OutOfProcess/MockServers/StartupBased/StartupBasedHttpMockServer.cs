using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.StartupBased
{
    public class HttpMockServer<T> : HttpMockServerBase where T : class
    {
        public HttpMockServer(HttpMockServerArgs mockServerArgs) : base(mockServerArgs)
        {
        }

        protected override IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup(typeof(T));
                    webBuilder.UseStartup<T>();
                });
        }
    }
}
