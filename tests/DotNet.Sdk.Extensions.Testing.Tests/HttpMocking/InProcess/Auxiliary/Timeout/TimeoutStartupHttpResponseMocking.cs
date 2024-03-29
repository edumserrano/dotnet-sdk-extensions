namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess.Auxiliary.Timeout;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Ignore for Startup type classes.")]
public class TimeoutStartupHttpResponseMocking
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ExceptionService>();
        services
            .AddHttpClient("named-client-with-low-timeout")
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(250);
            });
        services
            .AddHttpClient("named-client-with-high-timeout")
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(100);
            });
        services
            .AddHttpClient("polly-named-client-with-low-timeout")
            .AddHttpMessageHandler(_ =>
            {
                var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(250));
                return new PolicyHttpMessageHandler(timeoutPolicy);
            });
        services
            .AddHttpClient("polly-named-client-with-high-timeout")
            .AddHttpMessageHandler(_ =>
            {
                var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(100));
                return new PolicyHttpMessageHandler(timeoutPolicy);
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app
            .UseWhen(_ => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
            .UseRouting()
            .UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    if (exceptionHandlerPathFeature is not null)
                    {
                        var exceptionService = context.RequestServices.GetRequiredService<ExceptionService>();
                        exceptionService.AddException(exceptionHandlerPathFeature.Error);
                    }

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = MediaTypeNames.Text.Plain;
                    await context.Response.WriteAsync("An exception was thrown.", context.RequestAborted);
                });
            })
            .UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/named-client-with-low-timeout", async context =>
                {
                    var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                    var namedClient = httpClientFactory.CreateClient("named-client-with-low-timeout");
                    var response = await namedClient.GetAsync("https://named-client-with-timeout.com", context.RequestAborted);
                    await context.Response.WriteAsync($"Named http client (named-client-with-low-timeout) returned: {response.IsSuccessStatusCode}", context.RequestAborted);
                });
                endpoints.MapGet("/named-client-with-high-timeout", async context =>
                {
                    var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                    var namedClient = httpClientFactory.CreateClient("named-client-with-high-timeout");
                    var response = await namedClient.GetAsync("https://named-client-with-timeout.com", context.RequestAborted);
                    await context.Response.WriteAsync($"Named http client (named-client-with-high-timeout) returned: {response.IsSuccessStatusCode}", context.RequestAborted);
                });
                endpoints.MapGet("/polly-named-client-with-low-timeout", async context =>
                {
                    var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                    var namedClient = httpClientFactory.CreateClient("polly-named-client-with-low-timeout");
                    var response = await namedClient.GetAsync("https://polly-named-client.com", context.RequestAborted);
                    await context.Response.WriteAsync($"Named http client (polly-named-client-with-low-timeout) returned: {response.IsSuccessStatusCode}", context.RequestAborted);
                });
                endpoints.MapGet("/polly-named-client-with-high-timeout", async context =>
                {
                    var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                    var namedClient = httpClientFactory.CreateClient("polly-named-client-with-high-timeout");
                    var response = await namedClient.GetAsync("https://polly-named-client.com", context.RequestAborted);
                    await context.Response.WriteAsync($"Named http client (polly-named-client-with-high-timeout) returned: {response.IsSuccessStatusCode}", context.RequestAborted);
                });
            });
    }
}
