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

            // TODO for now disable https redirection because the test fails to run on linux based ci agent
            // In linux this test fails with error:
            // System.Net.Http.HttpRequestException : The SSL connection could not be established, see inner exception.
            // because dev certificate does not exist
            // Trying to set up the dev certificate with `dotnet dev-certs https --trust` does not work on linux
            // See https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-5.0&tabs=visual-studio#ssl-linux

            //app.UseHttpsRedirection(); 
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}