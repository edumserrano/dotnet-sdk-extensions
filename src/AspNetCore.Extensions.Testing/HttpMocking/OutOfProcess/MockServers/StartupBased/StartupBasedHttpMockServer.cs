using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.StartupBased
{
    public class HttpMockServer<T> : HttpMockServerBase where T : class
    {
        public override IHostBuilder CreateHostBuilder(string[] args)
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
