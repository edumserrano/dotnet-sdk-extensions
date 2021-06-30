using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess.Auxiliary.UseHttpMocks
{
    public class StartupHttpResponseMocking
    {
        public void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddHttpClient(); //add basic client
            _ = services.AddHttpClient("my-named-client"); // add named client
            _ = services.AddHttpClient<MyApiClient>("my-typed-client"); // add typed client with custom name
            _ = services.AddHttpClient("my-typed-client-2").AddTypedClient<MyApiClient>(); // add typed client with custom name, equivalent to the line above
            _ = services.AddHttpClient<MyApiClient>(); // add typed client, simple/most common way to declare one

            /*
             * The 'services.AddHttpClient<MyApiClient>();' should be the last registration for the typed
             * http client of type MyApiClient because that is what I want to get resolved on the handler
             * for the route '/typed-client'.
             *
             * The last registration is what gets resolved by default when you ask specifically for the type
             * MyApiClient from the service provider.
             *
             * If this changes some tests might break because the expected http client is not the one that
             * gets resolved on the route '/typed-client'
             *
             */
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = app
                .UseWhen(x => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    _ = endpoints.MapGet("/basic-client", async context =>
                      {
                          var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                          var basicClient = httpClientFactory.CreateClient();
                          var response = await basicClient.GetAsync("https://basic-client.com");
                          await context.Response.WriteAsync($"Basic http client returned: {response.IsSuccessStatusCode}");
                      });
                    _ = endpoints.MapGet("/named-client", async context =>
                      {
                          var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                          var namedClient = httpClientFactory.CreateClient("my-named-client");
                          var response = await namedClient.GetAsync("https://named-client.com");
                          await context.Response.WriteAsync($"Named http client (my-named-client) returned: {response.IsSuccessStatusCode}");
                      });
                    _ = endpoints.MapGet("/typed-client", async context =>
                      {
                          var typedClient = context.RequestServices.GetRequiredService<MyApiClient>();
                          var response = await typedClient.DoSomeHttpCall();
                          await context.Response.WriteAsync($"MyApiClient typed http client returned: {response}");
                      });
                    _ = endpoints.MapGet("/typed-client-with-custom-name", async context =>
                      {
                          var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                          var namedHttpClient = httpClientFactory.CreateClient("my-typed-client");
                          var typedHttpClientFactory = context.RequestServices.GetRequiredService<ITypedHttpClientFactory<MyApiClient>>();
                          var typedClientWithCustomName = typedHttpClientFactory.CreateClient(namedHttpClient);
                          var response = await typedClientWithCustomName.DoSomeHttpCall();
                          await context.Response.WriteAsync($"MyApiClient typed http client with custom name my-typed-client returned: {response}");
                      });
                    _ = endpoints.MapGet("/typed-client-with-custom-name-2", async context =>
                      {
                          var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                          var namedHttpClient = httpClientFactory.CreateClient("my-typed-client-2");
                          var typedHttpClientFactory = context.RequestServices.GetRequiredService<ITypedHttpClientFactory<MyApiClient>>();
                          var typedClientWithCustomName = typedHttpClientFactory.CreateClient(namedHttpClient);
                          var response = await typedClientWithCustomName.DoSomeHttpCall();
                          await context.Response.WriteAsync($"MyApiClient typed http client with custom name my-typed-client-2 returned: {response}");
                      });
                });
        }
    }
}
