using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Polly;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess.Auxiliary.Timeout
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Ignore for Startup type classes.")]
    public class TimeoutStartupHttpResponseMocking
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient("named-client");
            services
                .AddHttpClient("named-client-with-timeout")
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromMilliseconds(200);
                });
            services
                .AddHttpClient("polly-named-client")
                .AddHttpMessageHandler(_ =>
                {
                    var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(200));
                    return new PolicyHttpMessageHandler(timeoutPolicy);
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseWhen(_ => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/named-client", async context =>
                    {
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var namedClient = httpClientFactory.CreateClient("named-client");
                        var response = await namedClient.GetAsync("https://named-client.com");
                        await context.Response.WriteAsync($"Named http client (named-client) returned: {response.IsSuccessStatusCode}");
                    });
                    endpoints.MapGet("/named-client-with-timeout", async context =>
                    {
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var namedClient = httpClientFactory.CreateClient("named-client-with-timeout");
                        var response = await namedClient.GetAsync("https://named-client-with-timeout.com");
                        await context.Response.WriteAsync($"Named http client (named-client-with-timeout) returned: {response.IsSuccessStatusCode}");
                    });
                    endpoints.MapGet("/polly-named-client", async context =>
                    {
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var namedClient = httpClientFactory.CreateClient("polly-named-client");
                        var response = await namedClient.GetAsync("https://polly-named-client.com");
                        await context.Response.WriteAsync($"Named http client (polly-named-client) returned: {response.IsSuccessStatusCode}");
                    });
                });
        }
    }
}
