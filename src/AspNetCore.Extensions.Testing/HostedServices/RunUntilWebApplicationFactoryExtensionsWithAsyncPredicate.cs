using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AspNetCore.Extensions.Testing.HostedServices
{    
    /*
     * RunUntil WebApplicationFactory extension methods overloads
     * where the predicate is an async function.
     */
    public static partial class RunUntilWebApplicationFactoryExtensions
    {
        public static Task RunUntilAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            Func<Task<bool>> predicateAsync,
            CancellationToken runUntilCancellationToken = default) where T : class
        {
            var configureOptionsAction = new Action<RunUntilOptions>(DefaultConfigureOptionsDelegate);
            return webApplicationFactory.RunUntilAsync(predicateAsync, configureOptionsAction, runUntilCancellationToken);

            void DefaultConfigureOptionsDelegate(RunUntilOptions obj)
            {
                // default configure options delegate == do nothing
                // use default values of the RunUntilOptions
            }
        }

        public static Task RunUntilAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            Func<Task<bool>> predicateAsync,
            Action<RunUntilOptions> configureOptions,
            CancellationToken runUntilCancellationToken = default) where T : class
        {
            var defaultOptions = new RunUntilOptions();
            configureOptions(defaultOptions);
            return webApplicationFactory.RunUntilAsync(predicateAsync, defaultOptions, throwExceptionIfTimeout: true, runUntilCancellationToken);
        }
    }
}