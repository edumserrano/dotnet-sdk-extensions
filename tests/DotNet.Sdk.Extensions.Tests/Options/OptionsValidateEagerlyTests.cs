using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Options
{
    public class OptionsValidateEagerlyTests
    {
        /// <summary>
        /// This tests that the <see cref="IHost"/> will throw an exception when starting if the options
        /// validation fails.
        /// </summary>
        [Fact]
        public async Task ServerFailsToStartIfOptionsValidationFails()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<StartupOptionsValue>();
                })
                .Build();
            var validationException = await Should.ThrowAsync<ValidationException>(host.StartAsync());
            validationException.Message.ShouldBe("The SomeOption field is required.");
        }

        /// <summary>
        /// This tests that the <see cref="IHost"/> will start as expected if the options validation succeeds.
        /// </summary>
        [Fact]
        public void ServerStartsIfOptionsValidationSucceeds()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    var memoryConfigurationSource = new MemoryConfigurationSource
                    {
                        InitialData = new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>("SomeOption", "some value")
                            }
                    };
                    builder.Add(memoryConfigurationSource);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<StartupOptionsValue>();
                })
                .Build();
            Should.NotThrow(async () => await host.StartAsync());
        }

        /// <summary>
        /// Validates arguments for the <see cref="OptionsBuilderExtensions.ValidateEagerly{T}"/> extension method.
        /// </summary>
        [Fact]
        public void ValidatesArguments()
        {
            var optionsBuilderArgumentNullException = Should.Throw<ArgumentNullException>(() =>
            {
                OptionsBuilderExtensions.ValidateEagerly<MyOptions>(optionsBuilder: null!);
            });
            optionsBuilderArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'optionsBuilder')");
        }

        public class MyOptions
        {
            [Required]
            public string? SomeOption { get; set; }
        }

        public class MyOptionsValidation : IValidateOptions<MyOptions>
        {
            public ValidateOptionsResult Validate(string name, MyOptions options)
            {
                //this is doing data annotation validation but you can implement the validation however you like
                Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);
                return ValidateOptionsResult.Success;
            }
        }

        public class StartupOptionsValue
        {
            private readonly IConfiguration _configuration;

            public StartupOptionsValue(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public void ConfigureServices(IServiceCollection services)
            {
                // using IValidateOptions as recommended by https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1#ivalidateoptions-for-complex-validation
                services
                    .AddSingleton<IValidateOptions<MyOptions>, MyOptionsValidation>()
                    .AddOptions<MyOptions>()
                    .Bind(_configuration)
                    .ValidateEagerly();
            }

            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                app
                    .UseRouting()
                    .UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/", async context =>
                        {
                            var myOptions = context.RequestServices.GetRequiredService<MyOptions>();
                            await context.Response.WriteAsync(myOptions.SomeOption);
                        });
                    });
            }
        }
    }
}
