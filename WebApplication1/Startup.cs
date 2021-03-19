using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.HttpClient;
using DotNet.Sdk.Extensions.Polly.HttpClient.Options;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using DotNet.Sdk.Extensions.Polly;
using Polly.CircuitBreaker;

namespace WebApplication1
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication1", Version = "v1" });
            });

            services.AddPolicyRegistry((serviceProvider, registry) =>
            {
                registry.AddHttpClientRetryPolicy<GitHubPoliciesConfiguration>(policyKey: "GitHubRetry", serviceProvider);
                //registry.AddHttpClientTimeoutPolicy<GitHubPoliciesConfiguration>(policyKey: "GitHubTimeout", serviceProvider);
                //registry.AddHttpClientCircuitBreakerPolicy<GitHubPoliciesConfiguration>(policyKey: "GitHubCircuitBreaker", serviceProvider);

                //registry.AddHttpClientRetryPolicy(policyKey: "GitHubRetry", serviceProvider);
                registry.AddHttpClientTimeoutPolicy(policyKey: "GitHubTimeout", serviceProvider);
                registry.AddHttpClientCircuitBreakerPolicy(policyKey: "GitHubCircuitBreaker", serviceProvider);
            });

            services
                .AddHttpClient<GitHubClient>()
                //.AddRetryHandler(policyKey: "GitHubRetry", Configuration)
                //.AddRetryHandler(policyKey: "GitHubRetry", optionsBuilder=>optionsBuilder.Bind(Configuration.GetSection("GitHub")))
                .AddCircuitBreakerHandler(policyKey: "GitHubCircuitBreaker", optionsBuilder => optionsBuilder.Bind(Configuration.GetSection("GitHub")))
                .AddRetryHandler(policyKey: "GitHubRetry", Configuration)
                .AddTimeoutHandler(policyKey: "GitHubTimeout", optionsBuilder => optionsBuilder.Configure(options => options.TimeoutInSecs = 1))
                .AddHttpMessageHandler(() =>
                {
                    var testMessageHandler = new TestHttpMessageHandler();
                    testMessageHandler.MockHttpResponse(builder =>
                    {
                        builder.TimesOut(TimeSpan.FromSeconds(60));
                       
                        //builder.RespondWith(message =>
                        //{
                        //    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        //});
                    });
                    return testMessageHandler;
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication1 v1"));
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

    public class GitHubPoliciesConfiguration : IRetryPolicyConfiguration, ITimeoutPolicyConfiguration, ICircuitBreakerPolicyConfiguration
    {
        public Task OnRetry(
            RetryOptions retryOptions,
            DelegateResult<HttpResponseMessage> outcome,
            TimeSpan retryDelay,
            int retryNumber,
            Context pollyContext)
        {

            return Task.CompletedTask;
        }

        public Task OnTimeout(
            TimeoutOptions timeoutOptions,
            Context context,
            TimeSpan requestTimeout,
            Task timedOutTask,
            Exception exception)
        {

            return Task.CompletedTask;
        }

        public Task OnBreak(
            CircuitBreakerOptions circuitBreakerOptions,
            DelegateResult<HttpResponseMessage> lastOutcome,
            CircuitState previousState,
            TimeSpan durationOfBreak,
            Context context)
        {
            return Task.CompletedTask;
        }

        public Task OnHalfOpen(CircuitBreakerOptions circuitBreakerOptions)
        {
            return Task.CompletedTask;
        }

        public Task OnReset(CircuitBreakerOptions circuitBreakerOptions, Context context)
        {
            return Task.CompletedTask;
        }
    }

    public class GitHubClient
    {
        private readonly HttpClient _httpClient;

        public GitHubClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Get()
        {
            var response = await _httpClient.GetAsync("https://github.com");
            return "bla";
        }
    }
}
