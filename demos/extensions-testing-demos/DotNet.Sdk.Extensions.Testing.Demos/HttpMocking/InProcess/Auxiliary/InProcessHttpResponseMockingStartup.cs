using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.InProcess.Auxiliary
{
    public class InProcessHttpResponseMockingStartup
    {
        private readonly IConfiguration _configuration;

        public InProcessHttpResponseMockingStartup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<IMyApiClient, MyApiClient>();
            services.AddHttpClient("my_named_client");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseWhen(x => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/typed-client", async context =>
                    {
                        var typedClient = context.RequestServices.GetRequiredService<IMyApiClient>();
                        var response = await typedClient.DoSomeHttpCall();
                        await context.Response.WriteAsync($"ISomeApiClient typed http client returned: {response}");
                    });
                    endpoints.MapGet("/named-client", async context =>
                    {
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var namedClient = httpClientFactory.CreateClient("my_named_client");
                        var response = await namedClient.GetAsync("https://named-client.com");
                        await context.Response.WriteAsync($"Named http client (my_named_client) returned: {response.IsSuccessStatusCode}");
                    });
                    endpoints.MapGet("/basic-client", async context =>
                    {
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var basicClient = httpClientFactory.CreateClient();
                        var response = await basicClient.GetAsync("https://basic-client.com");
                        await context.Response.WriteAsync($"Basic http client returned: {response.IsSuccessStatusCode}");
                    });
                });
        }
    }
}
