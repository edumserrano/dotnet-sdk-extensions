using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.StartupBased.StartupWithControllers
{
    // This is equal to the startup provided by the asp.net core template when
    // creating a web api app. We are using it to show that the HttpMockServer
    // can be configured just like a real asp.net app.
    public class MyMockStartupWithControllers
    {
        public MyMockStartupWithControllers(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}