using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.StartupBased
{
    internal class StartupBasedHttpMockServer<T> : HttpMockServer where T : class
    {
        public StartupBasedHttpMockServer(HttpMockServerArgs mockServerArgs) : base(mockServerArgs)
        {
        }

        protected override IHostBuilder CreateHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    _ = webBuilder.UseStartup(typeof(T));
                    _ = webBuilder.UseStartup<T>();
                });
        }
    }
}
