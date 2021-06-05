using System;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using HttpClientWithResiliencePolicies.Controllers.Timeout;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

namespace HttpClientWithResiliencePolicies
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services
                .AddHttpClient<HttpClientWithTimeout>()
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = new Uri("https://github.com");
                })
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = 2;
                })
                .AddHttpMessageHandler(() =>
                {
                    var testMessageHandler = new TestHttpMessageHandler();
                    testMessageHandler.MockHttpResponse(builder =>
                    {
                        // the timeout policy will always trigger before this HttpClient message handler aborts.
                        builder.TimesOut(TimeSpan.FromSeconds(4));
                    });
                    return testMessageHandler;
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseWhen(x => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}
