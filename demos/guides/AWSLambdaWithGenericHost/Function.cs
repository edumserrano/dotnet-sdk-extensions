using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaWithGenericHost
{
    public class Function
    {
        private readonly MyApp _myApp;

        public Function()
        {
            var host = CreateHostBuilder(new[] { "" }).Build();
            _myApp = host.Services.GetRequiredService<MyApp>();
        }
        
        public string FunctionHandler(string input, ILambdaContext context)
        {
            return _myApp.Run(input);
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
