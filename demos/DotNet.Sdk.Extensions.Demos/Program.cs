using System.Threading.Tasks;
using CliFx;

namespace DotNet.Sdk.Extensions.Demos
{
    public class Program
    {
        public static async Task Main()
        {
            await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build()
                .RunAsync();
        }
    }
}
