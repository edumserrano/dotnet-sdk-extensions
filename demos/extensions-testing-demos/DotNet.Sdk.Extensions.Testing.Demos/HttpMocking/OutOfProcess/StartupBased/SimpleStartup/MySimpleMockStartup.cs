using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.StartupBased.SimpleStartup
{
    // This is a simple startup with inline middleware to configure the HttpMockServer responses
    public class MySimpleMockStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Use(async (httpContext, next) =>
            {
                if (httpContext.Request.Path.Equals("/users")
                    || httpContext.Request.Path.Equals("/admin")
                    || httpContext.Request.Path.Equals("/configuration"))
                {
                    httpContext.Response.StatusCode = StatusCodes.Status200OK;
                    await httpContext.Response.WriteAsync($"hello from {httpContext.Request.Path}");
                    return;
                }
                
                await next();
            });
            app.Run(httpContext =>
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Task.CompletedTask;
            });
        }
    }
}