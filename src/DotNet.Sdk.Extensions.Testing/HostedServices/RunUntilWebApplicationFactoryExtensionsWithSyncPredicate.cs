using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    /*
     * RunUntil WebApplicationFactory extension methods overloads
     * where the predicate is a sync function.
     */
    public static partial class RunUntilWebApplicationFactoryExtensions
    {
        public static Task RunUntilAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            Func<bool> predicate) where T : class
        {
            if (webApplicationFactory == null) throw new ArgumentNullException(nameof(webApplicationFactory));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            RunUntilPredicate predicateAsync = () => Task.FromResult(predicate());
            return webApplicationFactory.RunUntilAsync(predicateAsync);
        }

        public static Task RunUntilAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            Func<bool> predicate,
            Action<RunUntilOptions> configureOptions) where T : class
        {
            if (webApplicationFactory == null) throw new ArgumentNullException(nameof(webApplicationFactory));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            RunUntilPredicate predicateAsync = () => Task.FromResult(predicate());
            return webApplicationFactory.RunUntilAsync(predicateAsync, configureOptions, runUntilCancellationToken: default);
        }
    }
}