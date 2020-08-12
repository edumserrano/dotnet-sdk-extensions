using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.HttpMocking.MockServer
{
    public class HttpMockServer : IAsyncDisposable
    {
        private readonly List<HttpResponseMock> _httpResponseMocks;
        private IHost? _host;

        public HttpMockServer(List<HttpResponseMock> httpResponseMocks)
        {
            _httpResponseMocks = httpResponseMocks ?? throw new ArgumentNullException(nameof(httpResponseMocks));
        }

        public async Task Start(string[] args)
        {
            _host = CreateHostBuilder(args).Build();
            await _host.StartAsync();
        }

        public IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        var httpResponseMocksProvider = new HttpResponseMocksProvider(_httpResponseMocks);
                        services.AddSingleton(httpResponseMocksProvider);
                    });
                    webBuilder.UseStartup<HttpMockServerStartup>();
                });

        public async ValueTask DisposeAsync()
        {
            _host?.StopAsync();
            switch (_host)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
    
    public class HttpResponseMocksProvider
    {
        private readonly ICollection<HttpResponseMock> _httpResponseMocks;

        public HttpResponseMocksProvider(ICollection<HttpResponseMock> httpResponseMocks)
        {
            _httpResponseMocks = httpResponseMocks ?? throw new ArgumentNullException(nameof(httpResponseMocks));
        }

        public IEnumerable<HttpResponseMock> HttpResponseMocks => _httpResponseMocks.ToList();
    }

    public class HttpMockServer<T> : IAsyncDisposable where T : class
    {
        private IHost? _host;

        public async Task Start(string[] args)
        {
            _host = CreateHostBuilder(args).Build();
            await _host.StartAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup(typeof(T));
                    webBuilder.UseStartup<T>();
                });

        public async ValueTask DisposeAsync()
        {
            _host?.StopAsync();
            switch (_host)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }

    public class HttpMockServerStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env, 
            HttpResponseMocksProvider httpResponseMocksProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (httpContext, next) =>
            {
                foreach (var httpResponseMock in httpResponseMocksProvider.HttpResponseMocks)
                {
                    var result = await httpResponseMock.ExecuteAsync(httpContext);
                    if (result == HttpResponseMockResults.Executed)
                    {
                        return;
                    }
                }

                await next();
            });
            app.Run(httpContext =>
            {
                return Task.CompletedTask;
            });

            //app.UseRouting();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        await context.Response.WriteAsync("Hello World!");
            //    });
            //});
        }

    }
}
