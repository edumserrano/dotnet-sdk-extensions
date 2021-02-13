using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleAppWithGenericHost
{
    public class Program
    {
        static void Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
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
