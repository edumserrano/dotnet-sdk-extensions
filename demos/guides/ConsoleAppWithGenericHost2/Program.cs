using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleAppWithGenericHost2
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            await host.StartAsync();
            var myApp = host.Services.GetRequiredService<MyApp>();
            myApp.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddOptions<MyOptions>()
                        .Bind(context.Configuration.GetSection("MyOptions"));
                    services.AddSingleton<MyApp>();
                });
        }
    }
}
