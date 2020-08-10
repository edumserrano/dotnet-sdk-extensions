using AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.HttpMocking.MockServer
{
    public class HttpMockServer
    {
        public void Start(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }


        public HttpMockServer MockHttpResponse(HttpResponseMessageMock httpResponseMock)
        {
            _httpResponseMocks.Add(httpResponseMock);
            return this;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<HttpMockServerStartup>();
                });
    }

    public class HttpMockServerResponse
    {
        public void T()
        {
            DefaultHttpContext a = new DefaultHttpContext();
            var request = a.Request;
            var response = a.Response;
        }
    }



    public class HttpMockServerStartup
    {
        private readonly List<HttpResponseMessageMock> _httpResponseMocks = new List<HttpResponseMessageMock>();

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (httpContext, next) =>
            {
                foreach(var httpResponseMock in _httpResponseMocks)
                {
                    var result = await httpResponseMock.ExecuteAsync(httpContext.Request, httpContext.RequestAborted);
                    if(result.Status == HttpResponseMessageMockResults.Executed)
                    {
                        return result.HttpResponseMessage;
                    }
                }

                return next();
            });
            app.Run(httpContext =>
            {
                return Task.CompletedTask;
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }

    }
}
