using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.WebHostBuilders.Auxiliar
{
    public class StartupHttpResponseMocking
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient(); //add basic client
            services.AddHttpClient("my-named-client"); // add named client
            services.AddHttpClient<MyApiClient>(); // add typed client
            services.AddHttpClient<MyApiClient>("my-typed-client"); // add typed client with custom name
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseWhen(x => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/basic-client", async context =>
                    {
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var basicClient = httpClientFactory.CreateClient();
                        var response = await basicClient.GetAsync("https://basic-client.com");
                        await context.Response.WriteAsync($"Basic http client returned: {response.IsSuccessStatusCode}");
                    });
                    endpoints.MapGet("/named-client", async context =>
                    {
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var namedClient = httpClientFactory.CreateClient("my-named-client");
                        var response = await namedClient.GetAsync("https://named-client.com");
                        await context.Response.WriteAsync($"Named http client (my-named-client) returned: {response.IsSuccessStatusCode}");
                    });
                    endpoints.MapGet("/typed-client", async context =>
                    {
                        var typedClient = context.RequestServices.GetRequiredService<MyApiClient>();
                        var response = await typedClient.DoSomeHttpCall();
                        await context.Response.WriteAsync($"MyApiClient typed http client returned: {response}");
                    });
                    endpoints.MapGet("/typed-client-with-custom-name", async context =>
                    {
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var namedHttpClient = httpClientFactory.CreateClient("my-typed-client");
                        var typedHttpClientFactory = context.RequestServices.GetRequiredService<ITypedHttpClientFactory<MyApiClient>>();
                        var typedClientWithCustomName = typedHttpClientFactory.CreateClient(namedHttpClient);
                        var response = await typedClientWithCustomName.DoSomeHttpCall();
                        await context.Response.WriteAsync($"MyApiClient typed http client with custom name my-typed-client returned: {response}");
                    });
                });
        }
    }
}
