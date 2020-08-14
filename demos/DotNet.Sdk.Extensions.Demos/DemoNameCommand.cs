using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DotNet.Sdk.Extensions.Demos.Options.EagerValidateOptions;
using DotNet.Sdk.Extensions.Demos.Options.OptionsValue;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Demos
{
    [Command]
    public class DemoNameCommand : ICommand
    {
        private const string OPTIONS_VALUE_DEMO_NAME = "options-value";
        private const string EAGER_OPTIONS_VALIDATION_DEMO_NAME = "eager-options-validation";

        [CommandOption("demo", 'd', Description = "Demo name. Possible values are: '" + OPTIONS_VALUE_DEMO_NAME + "' and '" + EAGER_OPTIONS_VALIDATION_DEMO_NAME + "'.")]
        public string DemoName { get; set; } = default!;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            Type startupType;
            switch (DemoName)
            {
                case OPTIONS_VALUE_DEMO_NAME:
                    startupType = typeof(StartupOptionsValue);
                    break;
                case EAGER_OPTIONS_VALIDATION_DEMO_NAME:
                    startupType = typeof(StartupEagerOptionsValidation);
                    break;
                default:
                    throw new ArgumentException("Invalid demo name.");
            }

            await CreateHostBuilder(startupType).Build().RunAsync();
        }

        private IHostBuilder CreateHostBuilder(Type startupType)
        {
            return Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup(startupType);
                });
        }
    }
}