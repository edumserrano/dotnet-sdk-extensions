﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{    
    /*
     * RunUntil WebApplicationFactory extension methods overloads
     * where the predicate is an async function.
     */
    public static partial class RunUntilWebApplicationFactoryExtensions
    {
        public static Task RunUntilAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            RunUntilPredicateAsync predicateAsyncAsync,
            CancellationToken runUntilCancellationToken = default) where T : class
        {
            if (webApplicationFactory == null) throw new ArgumentNullException(nameof(webApplicationFactory));

            var configureOptionsAction = new Action<RunUntilOptions>(DefaultConfigureOptionsDelegate);
            return webApplicationFactory.RunUntilAsync(predicateAsyncAsync, configureOptionsAction, runUntilCancellationToken);

            void DefaultConfigureOptionsDelegate(RunUntilOptions obj)
            {
                // default configure options delegate == do nothing
                // use default values of the RunUntilOptions
            }
        }

        public static Task RunUntilAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            RunUntilPredicateAsync predicateAsyncAsync,
            Action<RunUntilOptions> configureOptions,
            CancellationToken runUntilCancellationToken = default) where T : class
        {
            if (webApplicationFactory == null) throw new ArgumentNullException(nameof(webApplicationFactory));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            var defaultOptions = new RunUntilOptions();
            configureOptions(defaultOptions);
            return webApplicationFactory.RunUntilAsync(predicateAsyncAsync, defaultOptions, throwExceptionIfTimeout: true, runUntilCancellationToken);
        }
    }
}