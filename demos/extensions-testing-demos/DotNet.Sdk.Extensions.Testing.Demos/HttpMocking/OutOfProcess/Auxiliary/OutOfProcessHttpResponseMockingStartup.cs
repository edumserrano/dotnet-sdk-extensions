using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.Auxiliary
{
    /*
     * This Startup uses a named HttpClient but the demo will work for any type of registered
     * HttpClient.
     *
     */
    public class OutOfProcessHttpResponseMockingStartup
    {
        private readonly IConfiguration _configuration;

        public OutOfProcessHttpResponseMockingStartup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions<HttpClientsOptions>();
            services.AddHttpClient("my_named_client");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseWhen(x => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/users", async context =>
                    {
                        var options = context.RequestServices.GetRequiredService<IOptions<HttpClientsOptions>>();
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var namedClient = httpClientFactory.CreateClient("my_named_client");
                        Console.WriteLine($"WTF: {options.Value.NamedClientBaseAddress}/users");
                        var response = await namedClient.GetAsync($"{options.Value.NamedClientBaseAddress}/users");
                        var responseBody = await response.Content.ReadAsStringAsync();
                        await context.Response.WriteAsync($"/users returned {response.StatusCode} with body {responseBody}");
                    });
                    endpoints.MapGet("/admin", async context =>
                    {
                        var options = context.RequestServices.GetRequiredService<IOptions<HttpClientsOptions>>();
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var namedClient = httpClientFactory.CreateClient("my_named_client");
                        var response = await namedClient.GetAsync($"{options.Value.NamedClientBaseAddress}/admin");
                        var responseBody = await response.Content.ReadAsStringAsync();
                        await context.Response.WriteAsync($"/admin returned {response.StatusCode} with body {responseBody}");
                    });
                    endpoints.MapGet("/configuration", async context =>
                    {
                        var options = context.RequestServices.GetRequiredService<IOptions<HttpClientsOptions>>();
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
                        var namedClient = httpClientFactory.CreateClient("my_named_client");
                        var response = await namedClient.GetAsync($"{options.Value.NamedClientBaseAddress}/configuration");
                        var responseBody = await response.Content.ReadAsStringAsync();
                        await context.Response.WriteAsync($"/configuration returned {response.StatusCode} with body {responseBody}");
                    });
                });
        }
    }
}
