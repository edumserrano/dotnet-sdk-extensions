using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetCore.Extensions.Options
{
    public static class OptionsBuilderExtensions
    {
        /*
         * first call services.AddOptions to setup the IOption<T> as you wish
         * if you rather just have injected T and not IOptions<T> then use the AddOptionsValue method below
         */
        public static OptionsBuilder<T> AddOptionsValue<T>(this OptionsBuilder<T> optionsBuilder) where T : class, new()
        {
            optionsBuilder.Services.AddOptionsValue<T>();
            return optionsBuilder;
        }

        private static IServiceCollection AddOptionsValue<T>(this IServiceCollection services) where T : class, new()
        {
            services.AddSingleton(serviceProvider =>
            {
                var options = serviceProvider.GetService<IOptions<T>>();
                return options.Value;
            });
            return services;
        }

        /*
         * At the moment there is no built-in mechanism to force eager validation on options so I've followed
         * the advice from https://github.com/dotnet/extensions/issues/459
         */
        public static OptionsBuilder<TOptions> ValidateEagerly<TOptions>(this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
        {
            optionsBuilder.Services.AddTransient<IStartupFilter, StartupOptionsValidation<TOptions>>();
            return optionsBuilder;
        }

        /*
         * Since this class is only to be used by the ValidateEagerly extension method I've added it here as a nested private class        
         */
        private class StartupOptionsValidation<T> : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return builder =>
                {
                    var options = builder.ApplicationServices.GetService(typeof(IOptions<>).MakeGenericType(typeof(T)));
                    if (options != null)
                    {
                        // Retrieve the value to trigger validation
                        var optionsValue = ((IOptions<object>)options).Value;
                    }

                    next(builder);
                };
            }
        }
    }
}
