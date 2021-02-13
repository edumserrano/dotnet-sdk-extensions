using DotNet.Sdk.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OptionsValue
{
    /*
     * Shows how to use the OptionsBuilder.AddOptionsValue and the IServiceCollection.AddOptionsValue extension methods.
     */
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISomeClass, SomeClass>();

            /*
             * One way of using AddOptionsValue is to configure the options value as you
             * 'normally' would and then use the OptionsBuilder.AddOptionsValue method.                          
             */
            services
                .AddOptions<MyOptions1>()
                .Bind(_configuration.GetSection("MyOptionsSection"))
                .AddOptionsValue();

            /*
             * There is also a 'shortcut' way to use it that might fit many scenarios whereby
             * you start by using the IServiceCollection.AddOptionsValue and then you get back
             * an OptionsBuilder if you want to further configure the options class.
             *
             */
            services.AddOptionsValue<MyOptions2>(_configuration, sectionName: "MyOptionsSection");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseWhen(x => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", async context =>
                    {
                        /* The implementation of ISomeClass takes the MyOptions1 and MyOptions2 classes
                         * as a dependency instead of taking in IOptions<MyOptions1> and IOptions<MyOptions2>.
                         * The ISomeClass.GetMessage method returns the value of MyOptions1.SomeOption and MyOptions2.SomeOption.
                         */
                        var someClass = context.RequestServices.GetRequiredService<ISomeClass>();
                        var message = someClass.GetMessage();
                        await context.Response.WriteAsync(message);
                    });
                });
        }
    }
}
