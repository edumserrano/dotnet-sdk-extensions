using AspNetCore.Extensions.Demos.Options;
using AspNetCore.Extensions.Demos.Options.EagerValidateOptions;
using AspNetCore.Extensions.Demos.Options.OptionsValue;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AspNetCore.Extensions.Demos
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup_OptionsValue>();
                    //webBuilder.UseStartup<Startup_EagerValidateOptions>();
                });
    }
}
