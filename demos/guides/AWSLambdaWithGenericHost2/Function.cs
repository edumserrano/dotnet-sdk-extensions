using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaWithGenericHost2
{
    public class Function
    {
        public async Task<string> FunctionHandler(string input, ILambdaContext context)
        {
            var host = CreateHostBuilder(new[] { "" }).Build();
            await host.StartAsync();
            var myApp = host.Services.GetRequiredService<MyApp>();
            var result =  myApp.Run(input);
            await host.StopAsync();
            return result;
        }

        private IHostBuilder CreateHostBuilder(string[] args)
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
