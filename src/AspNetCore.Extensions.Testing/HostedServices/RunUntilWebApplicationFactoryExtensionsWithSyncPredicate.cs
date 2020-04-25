﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AspNetCore.Extensions.Testing.HostedServices
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
            Func<Task<bool>> predicateAsync = () => Task.FromResult(predicate());
            return webApplicationFactory.RunUntilAsync(predicateAsync);
        }

        public static Task RunUntilAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            Func<bool> predicate,
            Action<RunUntilOptions> configureOptions) where T : class
        {
            Func<Task<bool>> predicateAsync = () => Task.FromResult(predicate());
            return webApplicationFactory.RunUntilAsync(predicateAsync, configureOptions, runUntilCancellationToken: default);
        }
    }
}