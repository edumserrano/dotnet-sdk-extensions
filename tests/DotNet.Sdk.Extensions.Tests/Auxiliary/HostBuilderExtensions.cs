using System.Collections.Generic;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNet.Sdk.Extensions.Tests.Auxiliary
{
    internal static class HostBuilderExtensions
    {
        // used to avoid logs from the server showing up on test output
        public static IHostBuilder SetDefaultLogLevel(this IHostBuilder hostBuilder, LogLevel logLevel)
        {
            return hostBuilder.ConfigureAppConfiguration((context, builder) =>
            {
                var memoryConfigurationSource = new MemoryConfigurationSource
                {
                    InitialData = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("Logging:LogLevel:Default", $"{logLevel}")
                    }
                };
                builder.Add(memoryConfigurationSource);
            });
        }
    }
}
