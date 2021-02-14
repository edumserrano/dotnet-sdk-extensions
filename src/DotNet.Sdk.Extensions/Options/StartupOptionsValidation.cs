using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Options
{
    internal class StartupOptionsValidation<T> : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                var optionsType = typeof(IOptions<>).MakeGenericType(typeof(T));
                var options = builder.ApplicationServices.GetService(optionsType);
                if (options is not null)
                {
                    // Retrieve the options value to trigger validation
                    _ = ((IOptions<object>)options).Value;
                }

                next(builder);
            };
        }
    }
}
