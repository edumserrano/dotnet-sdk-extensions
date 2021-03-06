using System;
using AmbientDataDemo.AmbientData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace AmbientDataDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AmbientDataDemo", Version = "v1" });
            });
            // add the ambient data to the IServiceCollection
            services.AddSingleton<IMyAmbientDataAccessor, MyAmbientDataAccessor>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AmbientDataDemo v1"));
            }

            app.Use(async (context, next) =>
            {
                // Set the ambient data to some value
                var ambientDataAccessor = context.RequestServices.GetRequiredService<IMyAmbientDataAccessor>();
                ambientDataAccessor.MyAmbientData = new MyAmbientData
                {
                    Value1 = DateTimeOffset.UtcNow.ToString(),
                    Value2 = Guid.NewGuid().ToString()
                };
                await next();
            });
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
