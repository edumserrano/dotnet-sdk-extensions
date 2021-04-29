﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions
{
    public static class ResilienceOptionsExtensions
    {
        public static OptionsBuilder<ResilienceOptions> AddHttpClientResilienceOptions(
            this IServiceCollection services,
            string name)
        {
            return services
                .AddSingleton<IValidateOptions<ResilienceOptions>, ResilienceOptionsValidation>()
                .AddOptions<ResilienceOptions>(name)
                .ValidateDataAnnotations();
        }

        public static ResilienceOptions GetHttpClientResilienceOptions(
            this IServiceProvider serviceProvider,
            string name)
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<ResilienceOptions>>();
            var options = optionsMonitor.Get(name);
            return options;
        }
    }
}

